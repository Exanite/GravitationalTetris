using System.Collections.Generic;
using System.Numerics;
using Diligent;
using Exanite.Core.Numerics;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.OldRendering;
using Exanite.Engine.Windowing;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class BloomSystem : GameSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    private float referenceResolutionHeight = 1080;

    private int maxIterationCount = 6;
    private float bloomIntensity = 0.05f;

    private Buffer<BloomDownUniformData> downUniformBuffer = null!;
    private IPipelineState downPipeline = null!;
    private IShaderResourceBinding downResources = null!;
    private IShaderResourceVariable? downTextureVariable;

    private Buffer<BloomUpUniformData> upUniformBuffer = null!;
    private IPipelineState upPipeline = null!;
    private IShaderResourceBinding upResources = null!;
    private IShaderResourceVariable? upTextureVariable;

    private Vector2Int currentSize;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly List<ColorRenderTexture2D> renderTextures = new();

    private readonly RenderingContext renderingContext;
    private readonly IResourceManager resourceManager;
    private readonly RenderingResourcesSystem renderingResourcesSystem;
    private readonly Window window;

    public BloomSystem(RenderingContext renderingContext, IResourceManager resourceManager, RenderingResourcesSystem renderingResourcesSystem, Window window)
    {
        this.renderingContext = renderingContext;
        this.resourceManager = resourceManager;
        this.renderingResourcesSystem = renderingResourcesSystem;
        this.window = window;
    }

    public void Setup()
    {
        ResizeRenderTextures();

        var renderDevice = renderingContext.RenderDevice;

        var vShader = resourceManager.GetResource(RenderingMod.ScreenVertexModule);
        var pShaderDown = resourceManager.GetResource(RenderingMod.BloomDownFragmentModule);
        var pShaderUp = resourceManager.GetResource(RenderingMod.BloomUpFragmentModule);

        {
            downUniformBuffer = new Buffer<BloomDownUniformData>(renderingContext, new BufferDesc()
            {
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.UniformBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
            }, 1);

            downPipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo()
            {
                GraphicsPipeline = new GraphicsPipelineDesc()
                {
                    PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                    NumRenderTargets = 1,
                    RTVFormats = [CommonTextureFormats.HdrTextureFormat],

                    RasterizerDesc = new RasterizerStateDesc() { CullMode = CullMode.Back, FrontCounterClockwise = true},
                    DepthStencilDesc = new DepthStencilStateDesc() { DepthEnable = false },

                    BlendDesc = new BlendStateDesc()
                    {
                        RenderTargets =
                        [
                            new RenderTargetBlendDesc
                            {
                                BlendEnable = true,
                                SrcBlend = BlendFactor.One,
                                DestBlend = BlendFactor.Zero,
                            },
                        ],
                    },
                },

                Vs = vShader.Value.Handle,
                Ps = pShaderDown.Value.Handle,
            });

            downPipeline.GetStaticVariableByName(ShaderType.Pixel, "Uniforms")?.Set(downUniformBuffer.Handle, SetShaderResourceFlags.None);

            downResources = downPipeline.CreateShaderResourceBinding(true);
            downTextureVariable = downResources.GetVariableByName(ShaderType.Pixel, "Texture");
        }

        {
            upUniformBuffer = new Buffer<BloomUpUniformData>(renderingContext, new BufferDesc()
            {
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.UniformBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
            }, 1);

            upPipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo()
            {
                GraphicsPipeline = new GraphicsPipelineDesc()
                {
                    PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                    NumRenderTargets = 1,
                    RTVFormats = [CommonTextureFormats.HdrTextureFormat],

                    RasterizerDesc = new RasterizerStateDesc() { CullMode = CullMode.Back, FrontCounterClockwise = true},
                    DepthStencilDesc = new DepthStencilStateDesc() { DepthEnable = false },

                    BlendDesc = new BlendStateDesc()
                    {
                        RenderTargets =
                        [
                            new RenderTargetBlendDesc()
                            {
                                BlendEnable = true,
                                SrcBlend = BlendFactor.One,
                                DestBlend = BlendFactor.One,
                            },
                        ],
                    },
                },

                Vs = vShader.Value.Handle,
                Ps = pShaderUp.Value.Handle,
            });

            upPipeline.GetStaticVariableByName(ShaderType.Pixel, "Uniforms")?.Set(upUniformBuffer.Handle, SetShaderResourceFlags.None);

            upResources = upPipeline.CreateShaderResourceBinding(true);
            upTextureVariable = upResources.GetVariableByName(ShaderType.Pixel, "Texture");
        }
    }

    public void Render()
    {
        ResizeRenderTextures();

        var deviceContext = renderingContext.DeviceContext;

        if (renderTextures.Count != 0)
        {
            // Down sample
            deviceContext.SetPipelineState(downPipeline);
            for (var i = 0; i < renderTextures.Count; i++)
            {
                var previousRenderTarget = i > 0 ? renderTextures[i - 1].RenderTarget : GetSourceRenderTexture().RenderTarget;
                var currentRenderTarget = renderTextures[i].RenderTarget;
                var currentTexture = renderTextures[i];

                downTextureVariable?.Set(previousRenderTarget, SetShaderResourceFlags.AllowOverwrite);
                renderTargets[0] = currentRenderTarget;

                using (downUniformBuffer.Map(MapType.Write, MapFlags.Discard, out var downUniformData))
                {
                    var textureDesc = currentTexture.Handle.GetDesc();
                    downUniformData[0].FilterStep = Vector2.One / textureDesc.GetSize();
                }

                deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);
                deviceContext.CommitShaderResources(downResources, ResourceStateTransitionMode.Transition);
                deviceContext.Draw(new DrawAttribs()
                {
                    NumVertices = 4,
                    Flags = DrawFlags.VerifyAll,
                });
            }

            var aspectRatio = window.AspectRatio;
            var step = 0.005f;
            var localUpUniformData = new BloomUpUniformData
            {
                FilterStep = new Vector2(step / aspectRatio, step),
                Alpha = 1,
            };

            using (upUniformBuffer.Map(MapType.Write, MapFlags.Discard, out var upUniformData))
            {
                upUniformData[0] = localUpUniformData;
            }

            // Up sample
            deviceContext.SetPipelineState(upPipeline);
            for (var i = renderTextures.Count - 2; i >= 0; i--)
            {
                var previousRenderTarget = renderTextures[i + 1].RenderTarget;
                var currentRenderTarget = renderTextures[i].RenderTarget;

                upTextureVariable?.Set(previousRenderTarget, SetShaderResourceFlags.AllowOverwrite);
                renderTargets[0] = currentRenderTarget;

                deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);
                deviceContext.CommitShaderResources(upResources, ResourceStateTransitionMode.Transition);
                deviceContext.Draw(new DrawAttribs()
                {
                    NumVertices = 4,
                    Flags = DrawFlags.VerifyAll,
                });
            }

            // Draw bloom to world RT
            renderTargets[0] = GetSourceRenderTexture().RenderTarget;
            deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);

            deviceContext.SetPipelineState(upPipeline);

            localUpUniformData.Alpha = bloomIntensity;
            using (upUniformBuffer.Map(MapType.Write, MapFlags.Discard, out var upUniformData))
            {
                upUniformData[0] = localUpUniformData;
            }

            upTextureVariable?.Set(renderTextures[0].RenderTarget, SetShaderResourceFlags.AllowOverwrite);
            deviceContext.CommitShaderResources(upResources, ResourceStateTransitionMode.Transition);
            deviceContext.Draw(new DrawAttribs()
            {
                NumVertices = 4,
                Flags = DrawFlags.VerifyAll,
            });
        }
    }

    public void Teardown()
    {
        downUniformBuffer.Dispose();
        downPipeline.Dispose();
        downResources.Dispose();

        upUniformBuffer.Dispose();
        upPipeline.Dispose();
        upResources.Dispose();

        foreach (var texture in renderTextures)
        {
            texture.Dispose();
        }
    }

    private void ResizeRenderTextures()
    {
        var sourceSize = GetSourceSize();
        if (currentSize != sourceSize)
        {
            foreach (var texture in renderTextures)
            {
                texture.Dispose();
            }

            renderTextures.Clear();

            CreateRenderTextures();
        }

        currentSize = sourceSize;
    }

    private void CreateRenderTextures()
    {
        var sourceSize = GetSourceSize();
        var sourceAspectRatio = (float)sourceSize.X / sourceSize.Y;

        // Use constant height to make bloom effect render the same regardless of resolution
        var width = referenceResolutionHeight * sourceAspectRatio;
        var height = referenceResolutionHeight;

        renderTextures.Clear();
        for (var i = 0; i < maxIterationCount; i++)
        {
            var iWidth = (int)width;
            var iHeight = (int)height;

            if (iWidth == 0 || iHeight == 0)
            {
                return;
            }

            renderTextures.Add(new ColorRenderTexture2D(renderingContext, new Vector2Int(iWidth, iHeight), CommonTextureFormats.HdrTextureFormat));

            width /= 2;
            height /= 2;
        }
    }

    // Temporary - Used to reduce coupling
    private ColorRenderTexture2D GetSourceRenderTexture()
    {
        return renderingResourcesSystem.WorldColor;
    }

    private Vector2Int GetSourceSize()
    {
        return renderingResourcesSystem.WorldColor.Handle.GetDesc().GetSize();
    }
}

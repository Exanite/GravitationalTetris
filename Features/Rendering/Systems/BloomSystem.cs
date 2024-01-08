using System;
using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class BloomSystem : ISetupSystem, IRenderSystem, ITeardownSystem
{
    private float referenceResolutionHeight = 1080;

    private int maxIterationCount = 6;
    private int iterationCount;
    private float bloomIntensity = 0.01f;

    private ISampler linearClampTextureSampler = null!;
    private ISampler pointClampTextureSampler = null!;

    private Buffer<BloomDownUniformData> downUniformBuffer = null!;
    private IPipelineState downPipeline = null!;
    private IShaderResourceBinding downResources = null!;
    private IShaderResourceVariable? downTextureVariable;

    private Buffer<BloomUpUniformData> upUniformBuffer = null!;
    private IPipelineState upPipeline = null!;
    private IShaderResourceBinding upResources = null!;
    private IShaderResourceVariable? upTextureVariable;

    private IPipelineState passthroughPipeline = null!;
    private IShaderResourceBinding passthroughResources = null!;
    private IShaderResourceVariable? passthroughTextureVariable;

    private uint previousWidth;
    private uint previousHeight;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly ITexture?[] renderTextures;
    private readonly ITextureView?[] renderTextureViews;

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;
    private readonly WorldRenderTextureSystem worldRenderTextureSystem;

    public BloomSystem(RendererContext rendererContext, IResourceManager resourceManager, WorldRenderTextureSystem worldRenderTextureSystem)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
        this.worldRenderTextureSystem = worldRenderTextureSystem;

        renderTextures = new ITexture[maxIterationCount];
        renderTextureViews = new ITextureView[maxIterationCount];
    }

    public void Setup()
    {
        ResizeRenderTextures();

        var renderDevice = rendererContext.RenderDevice;

        var vShader = resourceManager.GetResource<Shader>("Rendering:Screen.v.hlsl");
        var pShaderDown = resourceManager.GetResource<Shader>("Rendering:BloomDown.p.hlsl");
        var pShaderUp = resourceManager.GetResource<Shader>("Rendering:BloomUp.p.hlsl");
        var pShaderPassthrough = resourceManager.GetResource<Shader>("Rendering:Passthrough.p.hlsl");

        linearClampTextureSampler = renderDevice.CreateSampler(new SamplerDesc
        {
            MinFilter = FilterType.Linear, MagFilter = FilterType.Linear, MipFilter = FilterType.Linear,
            AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp,
        });

        pointClampTextureSampler = renderDevice.CreateSampler(new SamplerDesc
        {
            MinFilter = FilterType.Point, MagFilter = FilterType.Point, MipFilter = FilterType.Point,
            AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp,
        });

        {
            downUniformBuffer = new Buffer<BloomDownUniformData>("Bloom Down Uniform Buffer", rendererContext, new BufferDesc
            {
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.UniformBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
            });

            downPipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo
            {
                PSODesc = new PipelineStateDesc
                {
                    Name = "Bloom Down Shader Pipeline",
                    ResourceLayout = new PipelineResourceLayoutDesc
                    {
                        DefaultVariableType = ShaderResourceVariableType.Static,
                        Variables = new ShaderResourceVariableDesc[]
                        {
                            new()
                            {
                                ShaderStages = ShaderType.Pixel,
                                Name = "Texture",
                                Type = ShaderResourceVariableType.Mutable,
                            },
                        },
                    },
                },

                GraphicsPipeline = new GraphicsPipelineDesc
                {
                    PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                    NumRenderTargets = 1,
                    RTVFormats = new[] { TextureFormat.RGB32_Float },

                    RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.None },
                    DepthStencilDesc = new DepthStencilStateDesc { DepthEnable = false },

                    BlendDesc = new BlendStateDesc
                    {
                        RenderTargets = new RenderTargetBlendDesc[]
                        {
                            new()
                            {
                                BlendEnable = true,
                                SrcBlend = BlendFactor.One,
                                DestBlend = BlendFactor.Zero,
                            },
                        },
                    },
                },

                Vs = vShader.Value.Handle,
                Ps = pShaderDown.Value.Handle,
            });

            downPipeline.GetStaticVariableByName(ShaderType.Pixel, "TextureSampler")?.Set(linearClampTextureSampler, SetShaderResourceFlags.None);
            downPipeline.GetStaticVariableByName(ShaderType.Pixel, "Uniforms")?.Set(downUniformBuffer.Handle, SetShaderResourceFlags.None);

            downResources = downPipeline.CreateShaderResourceBinding(true);
            downTextureVariable = downResources.GetVariableByName(ShaderType.Pixel, "Texture");
        }

        {
            upUniformBuffer = new Buffer<BloomUpUniformData>("Bloom Up Uniform Buffer", rendererContext, new BufferDesc
            {
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.UniformBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
            });

            upPipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo
            {
                PSODesc = new PipelineStateDesc
                {
                    Name = "Bloom Up Shader Pipeline",
                    ResourceLayout = new PipelineResourceLayoutDesc
                    {
                        DefaultVariableType = ShaderResourceVariableType.Static,
                        Variables = new ShaderResourceVariableDesc[]
                        {
                            new()
                            {
                                ShaderStages = ShaderType.Pixel,
                                Name = "Texture",
                                Type = ShaderResourceVariableType.Mutable,
                            },
                        },
                    },
                },

                GraphicsPipeline = new GraphicsPipelineDesc
                {
                    PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                    NumRenderTargets = 1,
                    RTVFormats = new[] { TextureFormat.RGB32_Float },

                    RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.None },
                    DepthStencilDesc = new DepthStencilStateDesc { DepthEnable = false },

                    BlendDesc = new BlendStateDesc
                    {
                        RenderTargets = new RenderTargetBlendDesc[]
                        {
                            new()
                            {
                                BlendEnable = true,
                                SrcBlend = BlendFactor.One,
                                DestBlend = BlendFactor.One,
                            },
                        },
                    },
                },

                Vs = vShader.Value.Handle,
                Ps = pShaderUp.Value.Handle,
            });

            upPipeline.GetStaticVariableByName(ShaderType.Pixel, "TextureSampler")?.Set(linearClampTextureSampler, SetShaderResourceFlags.None);
            upPipeline.GetStaticVariableByName(ShaderType.Pixel, "Uniforms")?.Set(upUniformBuffer.Handle, SetShaderResourceFlags.None);

            upResources = upPipeline.CreateShaderResourceBinding(true);
            upTextureVariable = upResources.GetVariableByName(ShaderType.Pixel, "Texture");
        }

        {
            passthroughPipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo
            {
                PSODesc = new PipelineStateDesc
                {
                    Name = "Passthrough Shader Pipeline",
                    ResourceLayout = new PipelineResourceLayoutDesc
                    {
                        DefaultVariableType = ShaderResourceVariableType.Static,
                        Variables = new ShaderResourceVariableDesc[]
                        {
                            new()
                            {
                                ShaderStages = ShaderType.Pixel,
                                Name = "Texture",
                                Type = ShaderResourceVariableType.Mutable,
                            },
                        },
                    },
                },

                GraphicsPipeline = new GraphicsPipelineDesc
                {
                    PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                    NumRenderTargets = 1,
                    RTVFormats = new[] { TextureFormat.RGB32_Float },

                    RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.None },
                    DepthStencilDesc = new DepthStencilStateDesc { DepthEnable = false },
                },

                Vs = vShader.Value.Handle,
                Ps = pShaderPassthrough.Value.Handle,
            });

            passthroughPipeline.GetStaticVariableByName(ShaderType.Pixel, "TextureSampler")?.Set(pointClampTextureSampler, SetShaderResourceFlags.None);

            passthroughResources = passthroughPipeline.CreateShaderResourceBinding(true);
            passthroughTextureVariable = passthroughResources.GetVariableByName(ShaderType.Pixel, "Texture");
        }
    }

    public void Render()
    {
        ResizeRenderTextures();

        var deviceContext = rendererContext.DeviceContext;
        var swapChain = rendererContext.SwapChain;

        if (iterationCount != 0)
        {
            // Down sample
            deviceContext.SetPipelineState(downPipeline);
            for (var i = 0; i < iterationCount; i++)
            {
                var previousView = i > 0 ? renderTextureViews[i - 1] : GetSourceTextureView();
                var currentView = renderTextureViews[i];
                var currentTexture = renderTextures[i];

                downTextureVariable?.Set(previousView, SetShaderResourceFlags.AllowOverwrite);
                renderTargets[0] = currentView!;

                using (downUniformBuffer.Map(MapType.Write, MapFlags.Discard, out var downUniformData))
                {
                    var textureDesc = currentTexture!.GetDesc();

                    downUniformData[0].FilterStep = new Vector2(1f / textureDesc.Width, 1f / textureDesc.Height);
                }

                deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);
                deviceContext.CommitShaderResources(downResources, ResourceStateTransitionMode.Transition);
                deviceContext.Draw(new DrawAttribs
                {
                    NumVertices = 4,
                    Flags = DrawFlags.VerifyAll,
                });
            }

            var aspectRatio = (float)swapChain.GetDesc().Width / swapChain.GetDesc().Height;
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
            for (var i = iterationCount - 2; i >= 0; i--)
            {
                var previousView = renderTextureViews[i + 1];
                var currentView = renderTextureViews[i];

                upTextureVariable?.Set(previousView, SetShaderResourceFlags.AllowOverwrite);
                renderTargets[0] = currentView!;

                deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);
                deviceContext.CommitShaderResources(upResources, ResourceStateTransitionMode.Transition);
                deviceContext.Draw(new DrawAttribs
                {
                    NumVertices = 4,
                    Flags = DrawFlags.VerifyAll,
                });
            }

            // Draw bloom to world RT
            renderTargets[0] = GetSourceTextureView();
            deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);

            deviceContext.SetPipelineState(upPipeline);

            localUpUniformData.Alpha = bloomIntensity;
            using (upUniformBuffer.Map(MapType.Write, MapFlags.Discard, out var upUniformData))
            {
                upUniformData[0] = localUpUniformData;
            }

            upTextureVariable?.Set(renderTextureViews[0], SetShaderResourceFlags.AllowOverwrite);
            deviceContext.CommitShaderResources(upResources, ResourceStateTransitionMode.Transition);
            deviceContext.Draw(new DrawAttribs
            {
                NumVertices = 4,
                Flags = DrawFlags.VerifyAll,
            });
        }

        // Copy world to main RT, but without depth testing
        renderTargets[0] = swapChain.GetCurrentBackBufferRTV();
        deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);

        deviceContext.SetPipelineState(passthroughPipeline);
        passthroughTextureVariable?.Set(GetSourceTextureView(), SetShaderResourceFlags.AllowOverwrite);
        deviceContext.CommitShaderResources(passthroughResources, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });

        // Restore main RT settings
        deviceContext.SetRenderTargets(renderTargets, swapChain.GetDepthBufferDSV(), ResourceStateTransitionMode.Transition);
    }

    public void Teardown()
    {
        linearClampTextureSampler.Dispose();
        pointClampTextureSampler.Dispose();

        downUniformBuffer.Dispose();
        downPipeline.Dispose();
        downResources.Dispose();

        upUniformBuffer.Dispose();
        upPipeline.Dispose();
        upResources.Dispose();

        passthroughPipeline.Dispose();
        passthroughResources.Dispose();

        foreach (var texture in renderTextures)
        {
            texture?.Dispose();
        }
    }

    private void ResizeRenderTextures()
    {
        var sourceSize = GetSourceSize();
        if (previousWidth != sourceSize.Width || previousHeight != sourceSize.Height)
        {
            foreach (var texture in renderTextures)
            {
                texture?.Dispose();
            }

            Array.Clear(renderTextures);

            CreateRenderTextures();
        }

        previousWidth = sourceSize.Width;
        previousHeight = sourceSize.Height;
    }

    private void CreateRenderTextures()
    {
        var renderDevice = rendererContext.RenderDevice;

        var sourceSize = GetSourceSize();
        var sourceAspectRatio = (float)sourceSize.Width / sourceSize.Height;

        // Use constant height to make bloom effect render the same regardless of resolution
        var width = referenceResolutionHeight * sourceAspectRatio;
        var height = referenceResolutionHeight;

        iterationCount = 0;
        for (var i = 0; i < renderTextures.Length; i++)
        {
            var uWidth = (uint)width;
            var uHeight = (uint)height;

            if (uWidth == 0 || uHeight == 0)
            {
                return;
            }

            renderTextures[i] = renderDevice.CreateTexture(
                new TextureDesc
                {
                    Name = $"Bloom Render Texture {i + 1}/{renderTextures.Length}",
                    Type = ResourceDimension.Tex2d,
                    Width = uWidth,
                    Height = uHeight,
                    Format = TextureFormat.RGBA32_Float,
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = Usage.Default,
                });

            renderTextureViews[i] = renderTextures[i]!.GetDefaultView(TextureViewType.RenderTarget);
            iterationCount = i + 1;

            width /= 2;
            height /= 2;
        }
    }

    // Temporary - Used to reduce coupling
    private ITextureView GetSourceTextureView()
    {
        return worldRenderTextureSystem.WorldColorView;
    }

    private (uint Width, uint Height) GetSourceSize()
    {
        var desc = worldRenderTextureSystem.WorldColor.GetDesc();

        return (desc.Width, desc.Height);
    }
}

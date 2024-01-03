using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class BloomSystem : ISetupSystem, IRenderSystem, ITeardownSystem
{
    private int iterationCount = 6;

    private ISampler textureSampler = null!;

    private Buffer<BloomDownUniformData> downUniformBuffer = null!;
    private IPipelineState downPipeline = null!;
    private IShaderResourceBinding downResources = null!;
    private IShaderResourceVariable downTextureVariable = null!;

    private uint previousWidth;
    private uint previousHeight;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly ITexture[] renderTextures;
    private readonly ITextureView[] renderTextureViews;

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;
    private readonly WorldRenderTextureSystem worldRenderTextureSystem;

    public BloomSystem(RendererContext rendererContext, IResourceManager resourceManager, WorldRenderTextureSystem worldRenderTextureSystem)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
        this.worldRenderTextureSystem = worldRenderTextureSystem;

        renderTextures = new ITexture[iterationCount];
        renderTextureViews = new ITextureView[iterationCount];
    }

    public void Setup()
    {
        CreateRenderTextures();

        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;

        var vShader = resourceManager.GetResource<Shader>("Rendering:Bloom.v.hlsl");
        var pShaderDown = resourceManager.GetResource<Shader>("Rendering:BloomDown.p.hlsl");
        var pShaderUp = resourceManager.GetResource<Shader>("Rendering:BloomUp.p.hlsl");

        downUniformBuffer = new Buffer<BloomDownUniformData>("Bloom Down Uniform Buffer", rendererContext, new BufferDesc
        {
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.UniformBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
        });

        textureSampler = renderDevice.CreateSampler(new SamplerDesc
        {
            MinFilter = FilterType.Linear, MagFilter = FilterType.Linear, MipFilter = FilterType.Linear,
            AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp,
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
                RTVFormats = new[] { swapChain.GetDesc().ColorBufferFormat },

                RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.None },
                DepthStencilDesc = new DepthStencilStateDesc { DepthEnable = false },
            },

            Vs = vShader.Value.Handle,
            Ps = pShaderDown.Value.Handle,
        });

        downPipeline.GetStaticVariableByName(ShaderType.Pixel, "TextureSampler").Set(textureSampler, SetShaderResourceFlags.None);
        downPipeline.GetStaticVariableByName(ShaderType.Pixel, "Uniforms").Set(downUniformBuffer.Handle, SetShaderResourceFlags.None);

        downResources = downPipeline.CreateShaderResourceBinding(true);
        downTextureVariable = downResources.GetVariableByName(ShaderType.Pixel, "Texture");
    }

    public void Render()
    {
        ResizeRenderTextures();

        var deviceContext = rendererContext.DeviceContext;
        var swapChain = rendererContext.SwapChain;

        for (var i = 0; i < iterationCount; i++)
        {
            deviceContext.ClearRenderTarget(renderTextureViews[i], Vector4.Zero, ResourceStateTransitionMode.Transition);
        }

        deviceContext.SetPipelineState(downPipeline);
        for (var i = 0; i < iterationCount; i++)
        {
            var previousView = i > 0 ? renderTextureViews[i - 1] : worldRenderTextureSystem.WorldColorView;
            var currentView = renderTextureViews[i];
            var currentTexture = renderTextures[i];

            downTextureVariable.Set(previousView, SetShaderResourceFlags.AllowOverwrite);
            renderTargets[0] = currentView;

            using (downUniformBuffer.Map(MapType.Write, MapFlags.Discard, out var downUniformData))
            {
                var textureDesc = currentTexture.GetDesc();

                downUniformData[0].TextureResolution = new Vector2(textureDesc.Width, textureDesc.Height);
            }

            deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);
            deviceContext.CommitShaderResources(downResources, ResourceStateTransitionMode.Transition);
            deviceContext.Draw(new DrawAttribs
            {
                NumVertices = 4,
                Flags = DrawFlags.VerifyAll,
            });
        }

        downTextureVariable.Set(renderTextureViews[iterationCount - 1], SetShaderResourceFlags.AllowOverwrite);
        renderTargets[0] = swapChain.GetCurrentBackBufferRTV();

        deviceContext.SetRenderTargets(renderTargets, swapChain.GetDepthBufferDSV(), ResourceStateTransitionMode.Transition);
        deviceContext.CommitShaderResources(downResources, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });
    }

    public void Teardown()
    {
        downResources.Dispose();
        downPipeline.Dispose();
        downUniformBuffer.Dispose();

        foreach (var texture in renderTextures)
        {
            texture.Dispose();
        }
    }

    private void ResizeRenderTextures()
    {
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        if (previousWidth != swapChainDesc.Width || previousHeight != swapChainDesc.Height)
        {
            foreach (var texture in renderTextures)
            {
                texture.Dispose();
            }

            CreateRenderTextures();
        }
    }

    private void CreateRenderTextures()
    {
        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        var width = swapChainDesc.Width;
        var height = swapChainDesc.Height;

        // Todo Need to prevent zero width/height textures
        for (var i = 0; i < renderTextures.Length; i++)
        {
            width /= 2;
            height /= 2;

            renderTextures[i] = renderDevice.CreateTexture(
                new TextureDesc
                {
                    Name = $"Bloom Render Texture {i + 1}/{renderTextures.Length}",
                    Type = ResourceDimension.Tex2d,
                    Width = width,
                    Height = height,
                    Format = TextureFormat.RGBA32_Float,
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = Usage.Default,
                });

            renderTextureViews[i] = renderTextures[i].GetDefaultView(TextureViewType.RenderTarget);
        }

        previousWidth = swapChainDesc.Width;
        previousHeight = swapChainDesc.Height;
    }
}

using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class RenderWorldToMainSystem : ISetupSystem, IRenderSystem, ITeardownSystem
{
    private IPipelineState passthroughPipeline = null!;
    private IShaderResourceBinding passthroughResources = null!;
    private IShaderResourceVariable? passthroughTextureVariable;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;
    private readonly WorldRenderTextureSystem worldRenderTextureSystem;

    public RenderWorldToMainSystem(RendererContext rendererContext, IResourceManager resourceManager, WorldRenderTextureSystem worldRenderTextureSystem)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
        this.worldRenderTextureSystem = worldRenderTextureSystem;
    }

    public void Setup()
    {
        var renderDevice = rendererContext.RenderDevice;

        var vShader = resourceManager.GetResource<Shader>("Rendering:Screen.v.hlsl");
        var pShaderPassthrough = resourceManager.GetResource<Shader>("Rendering:Passthrough.p.hlsl");

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
                    ImmutableSamplers = new ImmutableSamplerDesc[]
                    {
                        new()
                        {
                            SamplerOrTextureName = "TextureSampler",
                            ShaderStages = ShaderType.Pixel,
                            Desc = new SamplerDesc
                            {
                                MinFilter = FilterType.Point, MagFilter = FilterType.Point, MipFilter = FilterType.Point,
                                AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp,
                            },
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

        passthroughResources = passthroughPipeline.CreateShaderResourceBinding(true);
        passthroughTextureVariable = passthroughResources.GetVariableByName(ShaderType.Pixel, "Texture");
    }

    public void Render()
    {
        var deviceContext = rendererContext.DeviceContext;
        var swapChain = rendererContext.SwapChain;

        renderTargets[0] = swapChain.GetCurrentBackBufferRTV();
        deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);

        passthroughTextureVariable?.Set(worldRenderTextureSystem.WorldColorView, SetShaderResourceFlags.AllowOverwrite);

        deviceContext.SetPipelineState(passthroughPipeline);
        deviceContext.CommitShaderResources(passthroughResources, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });

        deviceContext.SetRenderTargets(renderTargets, swapChain.GetDepthBufferDSV(), ResourceStateTransitionMode.Transition);
    }

    public void Teardown()
    {
        passthroughPipeline.Dispose();
        passthroughResources.Dispose();
    }
}

using Diligent;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Windowing;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class RenderWorldToMainSystem : EcsSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    private IPipelineState passthroughPipeline = null!;
    private IShaderResourceBinding passthroughResources = null!;
    private IShaderResourceVariable? passthroughTextureVariable;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;
    private readonly WorldRenderTextureSystem worldRenderTextureSystem;
    private readonly Window window;

    public RenderWorldToMainSystem(RendererContext rendererContext, IResourceManager resourceManager, WorldRenderTextureSystem worldRenderTextureSystem, Window window)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
        this.worldRenderTextureSystem = worldRenderTextureSystem;
        this.window = window;
    }

    public void Setup()
    {
        var renderDevice = rendererContext.RenderDevice;

        var vShader = resourceManager.GetResource(RenderingMod.ScreenShader);
        var pShaderPassthrough = resourceManager.GetResource(RenderingMod.PassthroughShader);

        passthroughPipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo()
        {
            PSODesc = new PipelineStateDesc()
            {
                ResourceLayout = new PipelineResourceLayoutDesc()
                {
                    DefaultVariableType = ShaderResourceVariableType.Static,
                    Variables =
                    [
                        new ShaderResourceVariableDesc()
                        {
                            ShaderStages = ShaderType.Pixel,
                            Name = "Texture",
                            Type = ShaderResourceVariableType.Mutable,
                        },
                    ],
                    ImmutableSamplers =
                    [
                        new ImmutableSamplerDesc()
                        {
                            SamplerOrTextureName = "TextureSampler",
                            ShaderStages = ShaderType.Pixel,
                            Desc = new SamplerDesc()
                            {
                                MinFilter = FilterType.Point, MagFilter = FilterType.Point, MipFilter = FilterType.Point,
                                AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp,
                            },
                        },
                    ],
                },
            },

            GraphicsPipeline = new GraphicsPipelineDesc()
            {
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                NumRenderTargets = 1,
                RTVFormats = [CommonTextureFormats.SrgbTextureFormat],

                RasterizerDesc = new RasterizerStateDesc() { CullMode = CullMode.Back, FrontCounterClockwise = true},
                DepthStencilDesc = new DepthStencilStateDesc() { DepthEnable = false },
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
        var swapChain = window.SwapChain;

        renderTargets[0] = swapChain.Handle.GetCurrentBackBufferRTV();
        deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);

        passthroughTextureVariable?.Set(worldRenderTextureSystem.WorldColor.RenderTarget, SetShaderResourceFlags.AllowOverwrite);

        deviceContext.SetPipelineState(passthroughPipeline);
        deviceContext.CommitShaderResources(passthroughResources, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs()
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });

        deviceContext.SetRenderTargets(renderTargets, swapChain.Handle.GetDepthBufferDSV(), ResourceStateTransitionMode.Transition);
    }

    public void Teardown()
    {
        passthroughPipeline.Dispose();
        passthroughResources.Dispose();
    }
}

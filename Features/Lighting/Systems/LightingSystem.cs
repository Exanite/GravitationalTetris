using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.GravitationalTetris.Features.Sprites;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Lighting.Systems;

public class LightingSystem : ISetupSystem, IRenderSystem
{
    private IPipelineState pipeline = null!;

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;

    public LightingSystem(RendererContext rendererContext, IResourceManager resourceManager)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
    }

    public void Setup()
    {
        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;

        var vShader = resourceManager.GetResource<Shader>("Lighting:Light.v.hlsl");
        var pShader = resourceManager.GetResource<Shader>("Lighting:Light.p.hlsl");

        pipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo
        {
            PSODesc = new PipelineStateDesc
            {
                Name = "Lighting Shader Pipeline",
            },

            GraphicsPipeline = new GraphicsPipelineDesc
            {
                InputLayout = SpriteInstanceData.Layout,
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                NumRenderTargets = 1,
                RTVFormats = new[] { swapChain.GetDesc().ColorBufferFormat },

                RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.None },
                DepthStencilDesc = new DepthStencilStateDesc { DepthEnable = false },
            },

            Vs = vShader.Value.Handle,
            Ps = pShader.Value.Handle,
        });
    }

    public void Render()
    {
        var deviceContext = rendererContext.DeviceContext;

        deviceContext.SetPipelineState(pipeline);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });
    }
}

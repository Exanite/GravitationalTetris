using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.GravitationalTetris.Features.Sprites;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Lighting.Systems;

public class LightingSystem : ISetupSystem, IRenderSystem
{
    private IPipelineState pipeline = null!;
    private IShaderResourceBinding shaderResourceBinding = null!;
    private IShaderResourceVariable textureVariable = null!;

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;
    private readonly WorldRenderTargetSystem worldRenderTargetSystem;

    public LightingSystem(RendererContext rendererContext, IResourceManager resourceManager, WorldRenderTargetSystem worldRenderTargetSystem)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
        this.worldRenderTargetSystem = worldRenderTargetSystem;
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
                ResourceLayout = new PipelineResourceLayoutDesc
                {
                    DefaultVariableType = ShaderResourceVariableType.Static,
                    Variables = new ShaderResourceVariableDesc[]
                    {
                        new ShaderResourceVariableDesc
                        {
                            ShaderStages = ShaderType.Pixel,
                            Name = "Texture",
                            Type = ShaderResourceVariableType.Mutable,
                        }
                    },
                    ImmutableSamplers = new ImmutableSamplerDesc[]
                    {
                        new()
                        {
                            SamplerOrTextureName = "Texture",
                            ShaderStages = ShaderType.Pixel,
                            Desc = new SamplerDesc
                            {
                                MinFilter = FilterType.Linear, MagFilter = FilterType.Linear, MipFilter = FilterType.Linear,
                                AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap,
                            },
                        },
                    },
                },
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

        shaderResourceBinding = pipeline.CreateShaderResourceBinding(true);
        textureVariable = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture");
    }

    public void Render()
    {
        var deviceContext = rendererContext.DeviceContext;

        textureVariable.Set(worldRenderTargetSystem.worldColorShaderResource, SetShaderResourceFlags.AllowOverwrite);

        deviceContext.SetPipelineState(pipeline);
        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });
    }
}

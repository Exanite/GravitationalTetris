using System;
using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.GravitationalTetris.Features.Resources;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering;

public class RenderingResourcesSystem : IInitializeSystem, IDisposable
{
    public Mesh Mesh = null!;
    public UniformBuffer<SpriteUniformData> UniformBuffer = null!;
    public IPipelineState Pipeline = null!;
    public IShaderResourceBinding ShaderResourceBinding = null!;

    private readonly RendererContext rendererContext;
    private readonly ResourceManager resourceManager;

    public RenderingResourcesSystem(RendererContext rendererContext, ResourceManager resourceManager)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
    }

    public void Initialize()
    {
        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;

        Mesh = Mesh.Create<VertexPositionUv>("Square mesh", rendererContext, new VertexPositionUv[]
        {
            new(new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 0)),
            new(new Vector3(0.5f, -0.5f, 0), new Vector2(1, 0)),
            new(new Vector3(0.5f, 0.5f, 0), new Vector2(1, 1)),
            new(new Vector3(-0.5f, 0.5f, 0), new Vector2(0, 1)),
        }, new uint[]
        {
            2, 1, 0,
            3, 2, 0,
        });

        UniformBuffer = new UniformBuffer<SpriteUniformData>("Sprite uniform buffer", rendererContext, new BufferDesc
        {
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.UniformBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
        });

        var vShader = resourceManager.GetResource(BaseMod.SpriteVShader);
        var pShader = resourceManager.GetResource(BaseMod.SpritePShader);

        Pipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo
        {
            PSODesc = new PipelineStateDesc
            {
                Name = "Sprite PSO",
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
                            Desc = new SamplerDesc
                            {
                                MinFilter = FilterType.Point, MagFilter = FilterType.Point, MipFilter = FilterType.Point,
                                AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp,
                            },
                            SamplerOrTextureName = "Texture",
                            ShaderStages = ShaderType.Pixel,
                        },
                    },
                },
            },
            Vs = vShader.Value.Handle,
            Ps = pShader.Value.Handle,
            GraphicsPipeline = new GraphicsPipelineDesc
            {
                InputLayout = VertexPositionUv.Layout,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.Front },
                DepthStencilDesc = new DepthStencilStateDesc { DepthEnable = true },
                BlendDesc = new BlendStateDesc
                {
                    RenderTargets = new RenderTargetBlendDesc[]
                    {
                        new()
                        {
                            BlendEnable = true,
                            SrcBlend = BlendFactor.SrcAlpha,
                            DestBlend = BlendFactor.InvSrcAlpha,
                        },
                    },
                },
                NumRenderTargets = 1,
                RTVFormats = new[] { swapChain.GetDesc().ColorBufferFormat },
                DSVFormat = swapChain.GetDesc().DepthBufferFormat,
            },
        });
        Pipeline.GetStaticVariableByName(ShaderType.Vertex, "Constants").Set(UniformBuffer.Buffer, SetShaderResourceFlags.None);

        ShaderResourceBinding = Pipeline.CreateShaderResourceBinding(true);
    }

    public void Dispose()
    {
        ShaderResourceBinding.Dispose();
        Pipeline.Dispose();
        UniformBuffer.Dispose();
        Mesh.Dispose();
    }
}

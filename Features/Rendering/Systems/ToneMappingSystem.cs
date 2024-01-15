using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Time;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class ToneMappingSystem : ISetupSystem, IRenderSystem, ITeardownSystem
{
    private Buffer<ToneMapUniformData> uniformBuffer = null!;
    private Reloadable<IPipelineState> pipeline = null!;
    private IShaderResourceBinding shaderResourceBinding = null!;
    private IShaderResourceVariable? textureVariable;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;
    private readonly WorldRenderTextureSystem worldRenderTextureSystem;
    private readonly SimulationTime time;

    public ToneMappingSystem(RendererContext rendererContext, IResourceManager resourceManager, WorldRenderTextureSystem worldRenderTextureSystem, SimulationTime time)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
        this.worldRenderTextureSystem = worldRenderTextureSystem;
        this.time = time;
    }

    public void Setup()
    {
        var renderDevice = rendererContext.RenderDevice;

        var vShader = resourceManager.GetResource<Shader>("Rendering:Screen.v.hlsl");
        var pShader = resourceManager.GetResource<Shader>("Rendering:ToneMap.p.hlsl");

        uniformBuffer = new Buffer<ToneMapUniformData>("Tone Map Uniform Buffer", rendererContext, new BufferDesc
        {
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.UniformBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
        });

        pipeline = new Reloadable<IPipelineState>(dependencies =>
        {
            dependencies.Add(vShader);
            dependencies.Add(pShader);

            return renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo
            {
                PSODesc = new PipelineStateDesc
                {
                    Name = "Post Process Shader Pipeline",
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
                Ps = pShader.Value.Handle,
            });
        });

        pipeline.Value.GetStaticVariableByName(ShaderType.Pixel, "Uniforms")?.Set(uniformBuffer.Handle, SetShaderResourceFlags.None);

        shaderResourceBinding = pipeline.Value.CreateShaderResourceBinding(true);
        textureVariable = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture");
    }

    public void Render()
    {
        var deviceContext = rendererContext.DeviceContext;

        // Disable depth buffer
        // Todo Figure out why DepthEnable=false doesn't work
        renderTargets[0] = worldRenderTextureSystem.WorldColorView;
        deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);

        using (uniformBuffer.Map(MapType.Write, MapFlags.Discard, out var uniformData))
        {
            uniformData[0].Time = time.Time;
        }

        textureVariable?.Set(worldRenderTextureSystem.WorldColorView, SetShaderResourceFlags.AllowOverwrite);

        deviceContext.SetPipelineState(pipeline.Value);
        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });
    }

    public void Teardown()
    {
        shaderResourceBinding.Dispose();
        pipeline.Dispose();
        uniformBuffer.Dispose();
    }
}
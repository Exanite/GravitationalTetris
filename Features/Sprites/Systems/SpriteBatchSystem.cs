using System;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;
using Exanite.GravitationalTetris.Features.Resources;

namespace Exanite.GravitationalTetris.Features.Sprites.Systems;

public class SpriteBatchSystem : IInitializeSystem, IRenderSystem, IDisposable
{
    private ISampler textureSampler = null!;
    private Buffer<SpriteNonBatchedUniformData> uniformBuffer = null!;
    private Buffer<SpriteInstanceData> instanceBuffer = null!;

    private IPipelineState pipeline = null!;
    private IShaderResourceBinding shaderResourceBinding = null!;
    private IShaderResourceVariable textureVariable = null!;

    private float initialZ = -500;
    private float incrementZ = 0.01f;

    private float currentZ;

    private SpriteBeginDrawOptions beginDrawOptions;

    private readonly RendererContext rendererContext;
    private readonly ResourceManager resourceManager;

    public SpriteBatchSystem(RendererContext rendererContext, ResourceManager resourceManager)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;

        currentZ = initialZ;
    }

    public void Initialize()
    {
        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;

        uniformBuffer = new Buffer<SpriteNonBatchedUniformData>("Sprite uniform buffer", rendererContext, new BufferDesc
        {
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.UniformBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
        });

        instanceBuffer = new Buffer<SpriteInstanceData>("Sprite instance buffer", rendererContext, new BufferDesc
        {
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.UniformBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
        });

        textureSampler = renderDevice.CreateSampler(new SamplerDesc
        {
            MinFilter = FilterType.Point, MagFilter = FilterType.Point, MipFilter = FilterType.Point,
            AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp,
        });

        var vShader = resourceManager.GetResource(BaseMod.SpriteVShader);
        var pShader = resourceManager.GetResource(BaseMod.SpritePShader);

        pipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo
        {
            PSODesc = new PipelineStateDesc
            {
                Name = "Sprite Shader Pipeline",
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
                InputLayout = VertexPositionUv.Layout,
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                NumRenderTargets = 1,
                RTVFormats = new[] { swapChain.GetDesc().ColorBufferFormat },
                DSVFormat = swapChain.GetDesc().DepthBufferFormat,

                RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.None },
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
            },

            Vs = vShader.Value.Handle,
            Ps = pShader.Value.Handle,
        });

        pipeline.GetStaticVariableByName(ShaderType.Pixel, "TextureSampler").Set(textureSampler, SetShaderResourceFlags.None);
        pipeline.GetStaticVariableByName(ShaderType.Vertex, "Constants").Set(uniformBuffer.Handle, SetShaderResourceFlags.None);

        shaderResourceBinding = pipeline.CreateShaderResourceBinding(true);
        textureVariable = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture");
    }

    public void Render()
    {
        currentZ = initialZ;
    }

    public void Dispose()
    {
        shaderResourceBinding.Dispose();
        textureSampler.Dispose();
        pipeline.Dispose();
        uniformBuffer.Dispose();
        instanceBuffer.Dispose();
    }

    public void Begin(SpriteBeginDrawOptions options)
    {
        beginDrawOptions = options;
    }

    public void Draw(SpriteDrawOptions options)
    {
        var deviceContext = rendererContext.DeviceContext;

        textureVariable.Set(options.Texture.DefaultView, SetShaderResourceFlags.AllowOverwrite);

        using (uniformBuffer.Map(MapType.Write, MapFlags.Discard, out var uniformData))
        {
            // Hack for implementing sprite sorting based on draw order
            if (options.World.Translation.Z == 0)
            {
                options.World.M43 = currentZ;
                currentZ += incrementZ;
            }

            uniformData[0] = new SpriteNonBatchedUniformData
            {
                World = options.World,
                View = beginDrawOptions.View,
                Projection = beginDrawOptions.Projection,

                Color = options.Color,

                Offset = options.Offset,
                Size = options.Size,
            };
        }

        deviceContext.SetPipelineState(pipeline);
        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
        });
    }

    public void End()
    {

    }
}

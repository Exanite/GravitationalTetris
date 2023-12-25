using System;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;
using Exanite.GravitationalTetris.Features.Resources;

namespace Exanite.GravitationalTetris.Features.Sprites.Systems;

public class SpriteBatchSystem : IInitializeSystem, IRenderSystem, IDisposable
{
    private const int MaxSpritesPerBatch = 1024;

    private ISampler textureSampler = null!;

    private Buffer<SpriteUniformData> uniformBuffer = null!;
    private Buffer<SpriteInstanceData> instanceBuffer = null!;
    private SpriteInstanceData[] instanceDataCache = new SpriteInstanceData[MaxSpritesPerBatch];

    private IPipelineState pipeline = null!;
    private IShaderResourceBinding shaderResourceBinding = null!;
    private IShaderResourceVariable textureVariable = null!;

    private float initialZ = -500;
    private float incrementZ = 0.01f;

    private int spritesDrawnThisFrame;
    private int spritesDrawnThisBatch;

    private SpriteBeginDrawOptions currentBeginDrawOptions;
    private Texture2D? currentTexture;

    private readonly IBuffer[] vertexBuffers = new IBuffer[1];
    private readonly ulong[] vertexOffsets = new ulong[1];

    private readonly RendererContext rendererContext;
    private readonly ResourceManager resourceManager;

    public SpriteBatchSystem(RendererContext rendererContext, ResourceManager resourceManager)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
    }

    public void Initialize()
    {
        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;

        uniformBuffer = new Buffer<SpriteUniformData>("Sprite uniform buffer", rendererContext, new BufferDesc
        {
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.UniformBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
        });

        instanceBuffer = new Buffer<SpriteInstanceData>("Sprite instance buffer", rendererContext, new BufferDesc
        {
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.VertexBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
        }, MaxSpritesPerBatch);

        textureSampler = renderDevice.CreateSampler(new SamplerDesc
        {
            MinFilter = FilterType.Point, MagFilter = FilterType.Point, MipFilter = FilterType.Point,
            AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp,
        });

        vertexBuffers[0] = instanceBuffer.Handle;

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
                InputLayout = SpriteInstanceData.Layout,
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
        pipeline.GetStaticVariableByName(ShaderType.Vertex, "Uniforms").Set(uniformBuffer.Handle, SetShaderResourceFlags.None);

        shaderResourceBinding = pipeline.CreateShaderResourceBinding(true);
        textureVariable = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture");
    }

    public void Render()
    {
        spritesDrawnThisFrame = 0;
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
        if (options != currentBeginDrawOptions)
        {
            End();
        }

        currentBeginDrawOptions = options;

        using (uniformBuffer.Map(MapType.Write, MapFlags.Discard, out var uniformData))
        {
            uniformData[0] = new SpriteUniformData()
            {
                View = currentBeginDrawOptions.View,
                Projection = currentBeginDrawOptions.Projection,
            };
        }
    }

    public void Draw(SpriteDrawOptions options)
    {
        if (spritesDrawnThisBatch == MaxSpritesPerBatch)
        {
            End();
        }

        if (options.Texture != currentTexture)
        {
            End();

            textureVariable.Set(options.Texture.DefaultView, SetShaderResourceFlags.AllowOverwrite);
            currentTexture = options.Texture;
        }

        // Hack for implementing sprite sorting based on draw order
        options.World.M43 = initialZ + incrementZ * spritesDrawnThisFrame;

        instanceDataCache[spritesDrawnThisBatch] = new SpriteInstanceData
        {
            World = options.World,

            Color = options.Color,
            Offset = options.Offset,

            Size = options.Size,
        };

        spritesDrawnThisFrame++;
        spritesDrawnThisBatch++;
    }

    public void End()
    {
        if (spritesDrawnThisBatch == 0)
        {
            return;
        }

        var deviceContext = rendererContext.DeviceContext;

        using (instanceBuffer.Map(MapType.Write, MapFlags.Discard, out var instanceData))
        {
            for (var i = 0; i < spritesDrawnThisBatch; i++)
            {
                instanceData[i] = instanceDataCache[i];
            }
        }

        deviceContext.SetPipelineState(pipeline);
        deviceContext.SetVertexBuffers(0, vertexBuffers, vertexOffsets, ResourceStateTransitionMode.Transition);
        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
            NumInstances = (uint)spritesDrawnThisBatch,
            Flags = DrawFlags.VerifyAll,
        });

        spritesDrawnThisBatch = 0;
    }
}

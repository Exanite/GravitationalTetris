using System;
using System.Numerics;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;
using Exanite.GravitationalTetris.Features.Resources;
using ValueType = Diligent.ValueType;

namespace Exanite.GravitationalTetris.Features.Sprites.Systems;

public class SpriteBatchSystem : IInitializeSystem, IRenderSystem, IDisposable
{
    private Mesh mesh = null!;
    private ISampler textureSampler = null!;
    private Buffer<SpriteUniformData> uniformBuffer = null!;

    private IPipelineState pipeline = null!;
    private IShaderResourceBinding shaderResourceBinding = null!;
    private IShaderResourceVariable textureVariable = null!;

    private float initialZ = -500;
    private float incrementZ = 0.01f;

    private float currentZ;

    private readonly IBuffer[] vertexBuffers = new IBuffer[1];
    private readonly ulong[] vertexOffsets = new ulong[1];

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

        mesh = Mesh.Create<VertexPositionUv>("Square mesh", rendererContext, new VertexPositionUv[]
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

        vertexBuffers[0] = mesh.VertexBuffer;

        uniformBuffer = new Buffer<SpriteUniformData>("Sprite uniform buffer", rendererContext, new BufferDesc
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
        mesh.Dispose();
    }

    public void DrawSprite(Texture2D texture, SpriteUniformData spriteUniformData)
    {
        var deviceContext = rendererContext.DeviceContext;

        textureVariable.Set(texture.DefaultView, SetShaderResourceFlags.AllowOverwrite);

        using (uniformBuffer.Map(MapType.Write, MapFlags.Discard, out var uniformData))
        {
            // Hack for implementing sprite sorting based on draw order
            if (spriteUniformData.World.Translation.Z == 0)
            {
                spriteUniformData.World.M43 = currentZ;
                currentZ += incrementZ;
            }

            uniformData[0] = spriteUniformData;
        }

        deviceContext.SetPipelineState(pipeline);
        deviceContext.SetVertexBuffers(0, vertexBuffers, vertexOffsets, ResourceStateTransitionMode.Transition);
        deviceContext.SetIndexBuffer(mesh.IndexBuffer, 0, ResourceStateTransitionMode.Transition);
        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.DrawIndexed(new DrawIndexedAttribs
        {
            IndexType = ValueType.UInt32,
            NumIndices = (uint)mesh.IndexCount,
            Flags = DrawFlags.VerifyAll,
        });
    }
}

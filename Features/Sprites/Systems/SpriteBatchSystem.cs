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
    public Mesh Mesh = null!;
    public Buffer<SpriteUniformData> UniformBuffer = null!;
    public IPipelineState Pipeline = null!;
    public IShaderResourceBinding ShaderResourceBinding = null!;
    public ISampler TextureSampler = null!;

    private float initialZ = -500;
    private float incrementZ = 0.01f;

    private float currentZ;

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

        UniformBuffer = new Buffer<SpriteUniformData>("Sprite uniform buffer", rendererContext, new BufferDesc
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

        TextureSampler = renderDevice.CreateSampler(new SamplerDesc
        {
            MinFilter = FilterType.Point, MagFilter = FilterType.Point, MipFilter = FilterType.Point,
            AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp,
        });

        Pipeline.GetStaticVariableByName(ShaderType.Pixel, "TextureSampler").Set(TextureSampler, SetShaderResourceFlags.None);
        Pipeline.GetStaticVariableByName(ShaderType.Vertex, "Constants").Set(UniformBuffer.Handle, SetShaderResourceFlags.None);

        ShaderResourceBinding = Pipeline.CreateShaderResourceBinding(true);
    }

    public void Render()
    {
        currentZ = initialZ;
    }

    public void Dispose()
    {
        ShaderResourceBinding.Dispose();
        TextureSampler.Dispose();
        Pipeline.Dispose();
        UniformBuffer.Dispose();
        Mesh.Dispose();
    }

    public void DrawSprite(Texture2D texture, SpriteUniformData spriteUniformData)
    {
        var deviceContext = rendererContext.DeviceContext;

        ShaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture").Set(texture.DefaultView, SetShaderResourceFlags.AllowOverwrite);

        using (UniformBuffer.Map(MapType.Write, MapFlags.Discard, out var uniformData))
        {
            // Hack for implementing sprite sorting based on draw order
            if (spriteUniformData.World.Translation.Z == 0)
            {
                spriteUniformData.World.M43 = currentZ;
                currentZ += incrementZ;
            }

            uniformData[0] = spriteUniformData;
        }

        deviceContext.SetPipelineState(Pipeline);
        deviceContext.SetVertexBuffers(0, new[] { Mesh.VertexBuffer }, new[] { 0ul }, ResourceStateTransitionMode.Transition);
        deviceContext.SetIndexBuffer(Mesh.IndexBuffer, 0, ResourceStateTransitionMode.Transition);
        deviceContext.CommitShaderResources(ShaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.DrawIndexed(new DrawIndexedAttribs
        {
            IndexType = ValueType.UInt32,
            NumIndices = 36,
            Flags = DrawFlags.VerifyAll,
        });
    }
}

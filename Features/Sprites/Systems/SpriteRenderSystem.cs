using Arch.System;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Sprites.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Exanite.ResourceManagement;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.System.SourceGenerator;
using Diligent;
using ValueType = Diligent.ValueType;
using Exanite.GravitationalTetris.Features.Resources;

namespace Exanite.GravitationalTetris.Features.Sprites.Systems;

public partial class SpriteRenderSystem : EcsSystem, IInitializeSystem, IRenderSystem, IDisposable
{
    private Mesh mesh = null!;
    private IBuffer uniformBuffer = null!;
    private IPipelineState pipeline = null!;
    private IShaderResourceBinding shaderResourceBinding = null!;

    private readonly RendererContext rendererContext;
    private readonly ResourceManager resourceManager;

    public SpriteRenderSystem(RendererContext rendererContext, ResourceManager resourceManager)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
    }

    public void Initialize()
    {
        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;

        mesh = Mesh.Create<VertexPositionUv>("Square mesh", rendererContext, new VertexPositionUv[]
        {
            new VertexPositionUv(new Vector3(-0.5f, -0.5f, 0), new Vector2(1, 1)),
            new VertexPositionUv(new Vector3(0.5f, -0.5f, 0), new Vector2(0, 1)),
            new VertexPositionUv(new Vector3(0.5f, 0.5f, 0), new Vector2(0, 0)),
            new VertexPositionUv(new Vector3(-0.5f, 0.5f, 0), new Vector2(1, 0)),
        }, new uint[]
        {
            2, 1, 0,
            3, 2, 0,
        });

        uniformBuffer = rendererContext.RenderDevice.CreateBuffer(new BufferDesc
        {
            Name = "Uniform buffer",
            Size = (ulong)Unsafe.SizeOf<Matrix4x4>(),
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.UniformBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
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
                RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.Back },
                DepthStencilDesc = new DepthStencilStateDesc { DepthEnable = true },
                BlendDesc = new BlendStateDesc()
                {
                    RenderTargets = new RenderTargetBlendDesc[]
                    {
                        new RenderTargetBlendDesc()
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
        pipeline.GetStaticVariableByName(ShaderType.Vertex, "Constants").Set(uniformBuffer, SetShaderResourceFlags.None);

        shaderResourceBinding = pipeline.CreateShaderResourceBinding(true);
    }

    public void Render()
    {
        DrawQuery(World);
    }

    public void Dispose()
    {
        shaderResourceBinding.Dispose();
        pipeline.Dispose();
        uniformBuffer.Dispose();
        mesh.Dispose();
    }



    [Query]
    [All<CameraComponent>]
    private void Draw(ref CameraProjectionComponent cameraProjection)
    {
        DrawSpritesQuery(World, ref cameraProjection);
    }

    [Query]
    private void DrawSprites([Data] ref CameraProjectionComponent cameraProjection, ref SpriteComponent sprite, ref TransformComponent transform)
    {
        var deviceContext = rendererContext.DeviceContext;

        var texture = sprite.Texture.Value;
        shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture").Set(texture.View, SetShaderResourceFlags.None);

        var world = Matrix4x4.CreateTranslation(transform.Position.X, transform.Position.Y, 0);
        var view = cameraProjection.View;
        var projection = cameraProjection.Projection;

        var worldViewProjection = world * view * projection;

        var mapUniformBuffer = deviceContext.MapBuffer<Matrix4x4>(uniformBuffer, MapType.Write, MapFlags.Discard);
        mapUniformBuffer[0] = worldViewProjection;
        deviceContext.UnmapBuffer(uniformBuffer, MapType.Write);

        deviceContext.SetPipelineState(pipeline);
        deviceContext.SetVertexBuffers(0, new[] { mesh.VertexBuffer }, new[] { 0ul }, ResourceStateTransitionMode.Transition);
        deviceContext.SetIndexBuffer(mesh.IndexBuffer, 0, ResourceStateTransitionMode.Transition);
        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.DrawIndexed(new DrawIndexedAttribs
        {
            IndexType = ValueType.UInt32,
            NumIndices = 36,
            Flags = DrawFlags.VerifyAll,
        });
    }
}

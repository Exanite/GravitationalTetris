using System;
using System.Collections.Generic;
using Diligent;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.OldRendering;
using Exanite.Engine.Timing;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class ToneMappingSystem : EcsSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    private Buffer<ToneMapUniformData> uniformBuffer = null!;
    private ReloadableHandle<IPipelineState> pipeline = null!;
    private IShaderResourceBinding shaderResourceBinding = null!;
    private IShaderResourceVariable? textureVariable;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;
    private readonly WorldRenderTextureSystem worldRenderTextureSystem;
    private readonly ITime time;

    public ToneMappingSystem(RendererContext rendererContext, IResourceManager resourceManager, WorldRenderTextureSystem worldRenderTextureSystem, ITime time)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
        this.worldRenderTextureSystem = worldRenderTextureSystem;
        this.time = time;
    }

    public void Setup()
    {
        var renderDevice = rendererContext.RenderDevice;

        var vShader = resourceManager.GetResource(RenderingMod.ScreenShader);
        var pShader = resourceManager.GetResource(RenderingMod.ToneMapShader);

        uniformBuffer = new Buffer<ToneMapUniformData>(rendererContext, new BufferDesc()
        {
            Usage = Usage.Dynamic,
            BindFlags = BindFlags.UniformBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
        }, 1);

        pipeline = new ReloadableHandle<IPipelineState>((List<IHandle> dependencies, out IPipelineState resource, out Action<IPipelineState> unloadAction) =>
        {
            dependencies.Add(vShader);
            dependencies.Add(pShader);

            resource = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo()
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
                    RTVFormats = [CommonTextureFormats.HdrTextureFormat],

                    RasterizerDesc = new RasterizerStateDesc() { CullMode = CullMode.Back, FrontCounterClockwise = true},
                    DepthStencilDesc = new DepthStencilStateDesc() { DepthEnable = false },
                },

                Vs = vShader.Value.Handle,
                Ps = pShader.Value.Handle,
            });

            pipeline.Value.GetStaticVariableByName(ShaderType.Pixel, "Uniforms")?.Set(uniformBuffer.Handle, SetShaderResourceFlags.None);

            shaderResourceBinding = pipeline.Value.CreateShaderResourceBinding(true);
            textureVariable = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture");

            unloadAction = resource =>
            {
                resource.Dispose();
                shaderResourceBinding.Dispose();
                shaderResourceBinding = null!;
                textureVariable = null!;
            };
        });
    }

    public void Render()
    {
        var deviceContext = rendererContext.DeviceContext;

        renderTargets[0] = worldRenderTextureSystem.WorldColor.RenderTarget;
        deviceContext.SetRenderTargets(renderTargets, worldRenderTextureSystem.WorldDepth.DepthStencil, ResourceStateTransitionMode.Transition);

        using (uniformBuffer.Map(MapType.Write, MapFlags.Discard, out var uniformData))
        {
            uniformData[0].Time = time.Time;
        }

        textureVariable?.Set(worldRenderTextureSystem.WorldColor.RenderTarget, SetShaderResourceFlags.AllowOverwrite);

        deviceContext.SetPipelineState(pipeline.Value);
        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs()
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });
    }

    public void Teardown()
    {
        uniformBuffer.Dispose();
        pipeline.Dispose();
    }
}

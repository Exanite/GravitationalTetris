using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class BloomSystem : ISetupSystem, IRenderSystem, ITeardownSystem
{
    private int iterationCount = 4;

    private IPipelineState downPipeline = null!;
    private IShaderResourceBinding shaderResourceBinding = null!;
    private IShaderResourceVariable textureVariable = null!;

    private uint previousWidth;
    private uint previousHeight;

    private readonly ITextureView[] renderTargets = new ITextureView[1];

    private readonly ITexture[] renderTextures = new ITexture[2];
    private readonly ITextureView[] renderTextureViews = new ITextureView[2];

    private readonly RendererContext rendererContext;
    private readonly IResourceManager resourceManager;
    private readonly WorldRenderTextureSystem worldRenderTextureSystem;

    public BloomSystem(RendererContext rendererContext, IResourceManager resourceManager, WorldRenderTextureSystem worldRenderTextureSystem)
    {
        this.rendererContext = rendererContext;
        this.resourceManager = resourceManager;
        this.worldRenderTextureSystem = worldRenderTextureSystem;
    }

    public void Setup()
    {
        CreateRenderTextures();

        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;

        var vShader = resourceManager.GetResource<Shader>("Rendering:Bloom.v.hlsl");
        var pShaderDown = resourceManager.GetResource<Shader>("Rendering:BloomDown.p.hlsl");
        var pShaderUp = resourceManager.GetResource<Shader>("Rendering:BloomUp.p.hlsl");

        downPipeline = renderDevice.CreateGraphicsPipelineState(new GraphicsPipelineStateCreateInfo
        {
            PSODesc = new PipelineStateDesc
            {
                Name = "Bloom Down Shader Pipeline",
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
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,

                NumRenderTargets = 1,
                RTVFormats = new[] { swapChain.GetDesc().ColorBufferFormat },

                RasterizerDesc = new RasterizerStateDesc { CullMode = CullMode.None },
                DepthStencilDesc = new DepthStencilStateDesc { DepthEnable = false },
            },

            Vs = vShader.Value.Handle,
            Ps = pShaderDown.Value.Handle,
        });

        shaderResourceBinding = downPipeline.CreateShaderResourceBinding(true);
        textureVariable = shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture");
    }

    public void Render()
    {
        ResizeRenderTextures();

        var deviceContext = rendererContext.DeviceContext;
        var swapChain = rendererContext.SwapChain;

        deviceContext.SetPipelineState(downPipeline);
        for (var i = 0; i < iterationCount; i++)
        {
            var previousView = renderTextureViews[(i + 1) % 2];

            if (i == 0)
            {
                textureVariable.Set(worldRenderTextureSystem.worldColorView, SetShaderResourceFlags.AllowOverwrite);
            }
            else
            {
                textureVariable.Set(previousView, SetShaderResourceFlags.AllowOverwrite);
            }

            renderTargets[0] = previousView;
            deviceContext.SetRenderTargets(renderTargets, null, ResourceStateTransitionMode.Transition);

            deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
            deviceContext.Draw(new DrawAttribs
            {
                NumVertices = 4,
                Flags = DrawFlags.VerifyAll,
            });
        }

        textureVariable.Set(renderTextureViews[iterationCount % 2], SetShaderResourceFlags.AllowOverwrite);
        renderTargets[0] = swapChain.GetCurrentBackBufferRTV();
        deviceContext.SetRenderTargets(renderTargets, swapChain.GetDepthBufferDSV(), ResourceStateTransitionMode.Transition);

        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.Draw(new DrawAttribs
        {
            NumVertices = 4,
            Flags = DrawFlags.VerifyAll,
        });
    }

    public void Teardown()
    {
        foreach (var texture in renderTextures)
        {
            texture.Dispose();
        }
    }

    private void ResizeRenderTextures()
    {
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        if (previousWidth != swapChainDesc.Width || previousHeight != swapChainDesc.Height)
        {
            foreach (var texture in renderTextures)
            {
                texture.Dispose();
            }

            CreateRenderTextures();
        }
    }

    private void CreateRenderTextures()
    {
        var renderDevice = rendererContext.RenderDevice;
        var swapChain = rendererContext.SwapChain;
        var swapChainDesc = swapChain.GetDesc();

        for (var i = 0; i < renderTextures.Length; i++)
        {
            renderTextures[i] = renderDevice.CreateTexture(
                new TextureDesc
                {
                    Name = $"Bloom Render Texture {i + 1}/{renderTextures.Length}",
                    Type = ResourceDimension.Tex2d,
                    Width = swapChainDesc.Width,
                    Height = swapChainDesc.Height,
                    Format = TextureFormat.RGBA32_Float,
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = Usage.Default,
                });

            renderTextureViews[i] = renderTextures[i].GetDefaultView(TextureViewType.RenderTarget);
        }

        previousWidth = swapChainDesc.Width;
        previousHeight = swapChainDesc.Height;
    }
}

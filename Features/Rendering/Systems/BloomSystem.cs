using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class BloomSystem : ISetupSystem, IRenderSystem, ITeardownSystem
{
    private int iterationCount = 4;

    private uint previousWidth;
    private uint previousHeight;

    private readonly ITexture[] renderTextures = new ITexture[2];
    private readonly ITextureView[] renderTextureViews = new ITextureView[2];

    private readonly RendererContext rendererContext;

    public BloomSystem(RendererContext rendererContext)
    {
        this.rendererContext = rendererContext;
    }

    public void Setup()
    {
        CreateRenderTextures();
    }

    public void Render()
    {
        ResizeRenderTextures();
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
                    Name = $"Bloom Render Texture {i + 1}/{iterationCount + 1}",
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

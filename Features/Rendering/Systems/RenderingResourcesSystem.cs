using System.Numerics;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Graphics;
using Exanite.Engine.Windowing;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class RenderingResourcesSystem : GameSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    public Texture2D WorldColor = null!;
    public Texture2D WorldDepth = null!;

    private readonly GraphicsContext graphicsContext;
    private readonly Window window;
    private readonly Swapchain swapchain;

    public RenderingResourcesSystem(GraphicsContext graphicsContext, Window window, Swapchain swapchain)
    {
        this.graphicsContext = graphicsContext;
        this.window = window;
        this.swapchain = swapchain;
    }

    public void Setup()
    {
        WorldColor = new Texture2D(graphicsContext, new TextureDesc2D()
        {
            Format = swapchain.Desc.Format,
            Size = swapchain.Texture.Desc.Size,
            Usages = ImageUsageFlags.ColorAttachmentBit,
        }, new TextureViewDesc2D()
        {
            Aspects = ImageAspectFlags.ColorBit,
        });

        WorldDepth = new Texture2D(graphicsContext, new TextureDesc2D()
        {
            Format = swapchain.Desc.Format,
            Size = swapchain.Texture.Desc.Size,
            Usages = ImageUsageFlags.DepthStencilAttachmentBit,
        }, new TextureViewDesc2D()
        {
            Aspects = ImageAspectFlags.DepthBit,
        });
    }

    public void Render()
    {
        var vk = graphicsContext.Vk;

        WorldColor.ResizeIfNeeded(window.Size);
        WorldDepth.ResizeIfNeeded(window.Size);

        renderTargets[0] = WorldColor.RenderTarget;

        deviceContext.SetRenderTargets(renderTargets, WorldDepth.DepthStencil, ResourceStateTransitionMode.Transition);
        deviceContext.ClearRenderTarget(WorldColor.RenderTarget, Vector4.Zero, ResourceStateTransitionMode.Transition);
        deviceContext.ClearDepthStencil(WorldDepth.DepthStencil, ClearDepthStencilFlags.Depth, 1, 0, ResourceStateTransitionMode.Transition);
    }

    public void Teardown()
    {
        WorldColor.Dispose();
        WorldDepth.Dispose();
    }
}

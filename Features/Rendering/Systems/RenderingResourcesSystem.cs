using System.Numerics;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Windowing;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public class RenderingResourcesSystem : EcsSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    public Texture2D WorldColor = null!;
    public Texture2D WorldDepth = null!;

    private readonly RenderingContext renderingContext;
    private readonly Window window;
    private readonly Swapchain swapchain;

    public RenderingResourcesSystem(RenderingContext renderingContext, Window window, Swapchain swapchain)
    {
        this.renderingContext = renderingContext;
        this.window = window;
        this.swapchain = swapchain;
    }

    public void Setup()
    {
        WorldColor = new Texture2D(renderingContext, new TextureDesc2D()
        {
            Format = swapchain.Desc.Format,
            Size = swapchain.Texture.Desc.Size,
            Usages = ImageUsageFlags.ColorAttachmentBit,
        }, new TextureViewDesc2D()
        {
            Aspects = ImageAspectFlags.ColorBit,
        });

        WorldDepth = new Texture2D(renderingContext, new TextureDesc2D()
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
        var vk = renderingContext.Vk;

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

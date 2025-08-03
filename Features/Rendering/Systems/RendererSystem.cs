using System;
using System.Numerics;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Graphics;
using Exanite.Engine.Windowing;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Sprites;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public partial class RendererSystem : GameSystem, ISetupSystem, IRenderSystem, ITeardownSystem, IDisposable
{
    public Texture2D WorldColor = null!;
    public Texture2D WorldDepth = null!;

    private readonly SpriteBatcher spriteBatcher;

    private readonly GraphicsContext graphicsContext;
    private readonly ResourceManager resourceManager;
    private readonly Window window;
    private readonly Swapchain swapchain;

    public RendererSystem(GraphicsContext graphicsContext, ResourceManager resourceManager, Window window, Swapchain swapchain)
    {
        this.graphicsContext = graphicsContext;
        this.resourceManager = resourceManager;
        this.window = window;
        this.swapchain = swapchain;

        spriteBatcher = new SpriteBatcher(graphicsContext, resourceManager);
    }

    public void Setup()
    {
        WorldColor = new Texture2D(graphicsContext, new TextureDesc2D()
        {
            Format = Format.R32G32B32A32Sfloat,
            Size = swapchain.Texture.Desc.Size,
            Usages = ImageUsageFlags.ColorAttachmentBit,
        }, new TextureViewDesc()
        {
            Aspects = ImageAspectFlags.ColorBit,
        });

        WorldDepth = new Texture2D(graphicsContext, new TextureDesc2D()
        {
            Format = Format.D32Sfloat,
            Size = swapchain.Texture.Desc.Size,
            Usages = ImageUsageFlags.DepthStencilAttachmentBit,
        }, new TextureViewDesc()
        {
            Aspects = ImageAspectFlags.DepthBit,
        });
    }

    public void Render()
    {
        var vk = graphicsContext.Vk;
        var commandBuffer = swapchain.CommandBuffer;

        // Resize world render targets
        WorldColor.ResizeIfNeeded(window.Size);
        WorldDepth.ResizeIfNeeded(window.Size);

        // Clear world render targets
        commandBuffer.AddBarrier(new TextureBarrier(WorldColor)
        {
            SrcStages = PipelineStageFlags2.ColorAttachmentOutputBit,
            SrcAccesses = AccessFlags2.ColorAttachmentWriteBit,

            DstStages = PipelineStageFlags2.ColorAttachmentOutputBit,
            DstAccesses = AccessFlags2.ColorAttachmentReadBit | AccessFlags2.ColorAttachmentWriteBit,

            SrcLayout = ImageLayout.Undefined,
            DstLayout = ImageLayout.AttachmentOptimal,
        });

        commandBuffer.AddBarrier(new TextureBarrier(WorldDepth)
        {
            SrcStages = PipelineStageFlags2.LateFragmentTestsBit,
            SrcAccesses = AccessFlags2.DepthStencilAttachmentWriteBit,

            DstStages = PipelineStageFlags2.EarlyFragmentTestsBit,
            DstAccesses = AccessFlags2.DepthStencilAttachmentReadBit | AccessFlags2.DepthStencilAttachmentWriteBit,

            SrcLayout = ImageLayout.Undefined,
            DstLayout = ImageLayout.AttachmentOptimal,
        });

        using (commandBuffer.BeginRenderPass(new RenderPassDesc([WorldColor], WorldDepth)))
        {
            commandBuffer.ClearColorAttachment(Vector4.Zero);
            commandBuffer.ClearDepthAttachment(0);
        }

        RenderCameraQuery(World, commandBuffer);
    }

    [Query]
    private void RenderCamera([Data] GraphicsCommandBuffer commandBuffer, ref ComponentCameraProjection cameraProjection)
    {
        // Render a test sprite
        using (var batch = spriteBatcher.Acquire(new SpriteUniformDrawSettings()
               {
                   CommandBuffer = commandBuffer,
                   ColorTarget = WorldColor,
                   DepthTarget = WorldDepth,
                   View = cameraProjection.View,
                   Projection = cameraProjection.Projection,
               }))
        {
            batch.Draw(new SpriteInstanceDrawSettings()
            {
                Texture = resourceManager.GetResource(BaseMod.Player).Value,
                Model = Matrix4x4.CreateScale(1, -1, 1),
            });
        }
    }

    public void Teardown()
    {
        WorldColor.Dispose();
        WorldDepth.Dispose();
    }

    public void Dispose()
    {
        spriteBatcher.Dispose();
    }
}

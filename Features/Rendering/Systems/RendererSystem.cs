using System;
using System.Numerics;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Graphics;
using Exanite.Engine.Graphics.Passes;
using Exanite.Engine.Timing;
using Exanite.Engine.Windowing;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Sprites;
using Exanite.GravitationalTetris.Features.Sprites.Components;
using Exanite.GravitationalTetris.Features.Tetris.Components;
using Exanite.GravitationalTetris.Features.Tetris.Systems;
using Exanite.GravitationalTetris.Features.Tiles;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public partial class RendererSystem : GameSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    public Texture2D WorldColor = null!;
    public Texture2D WorldDepth = null!;

    private IResourceHandle<Texture2D> emptyTileTexture;
    private IResourceHandle<Texture2D> placeholderTileTexture;

    private readonly SpriteBatcher spriteBatcher;
    private readonly CopyColorTexturePass copyWorldPass;
    private readonly CopyColorTexturePass copyUiPass;

    private DisposableCollection disposables = new();

    private readonly GraphicsContext graphicsContext;
    private readonly ResourceManager resourceManager;
    private readonly Window window;
    private readonly Swapchain swapchain;
    private readonly TetrisUiSystem tetrisUiSystem;
    private readonly GameTilemapData tilemap;
    private readonly ITime time;

    public RendererSystem(
        GraphicsContext graphicsContext,
        ResourceManager resourceManager,
        Window window,
        Swapchain swapchain,
        TetrisUiSystem tetrisUiSystem,
        GameTilemapData tilemap,
        ITime time)
    {
        this.graphicsContext = graphicsContext;
        this.resourceManager = resourceManager;
        this.window = window;
        this.swapchain = swapchain;
        this.tetrisUiSystem = tetrisUiSystem;
        this.tilemap = tilemap;
        this.time = time;

        emptyTileTexture = resourceManager.GetResource(BaseMod.TileNone);
        placeholderTileTexture = resourceManager.GetResource(BaseMod.TilePlaceholder);

        spriteBatcher = new SpriteBatcher(graphicsContext, resourceManager).AddTo(disposables);

        copyWorldPass = new CopyColorTexturePass(graphicsContext, resourceManager)
        {
            // TODO: This is a hack to make world color actually copy over
            // Currently all alpha values in the world color texture are 0 and this should be fixed
            Blend = new ShaderPipelineColorAttachmentBlendDesc()
            {
                ColorBlendOp = BlendOp.Add,
                SrcColorBlendFactor = BlendFactor.One,
                DstColorBlendFactor = BlendFactor.Zero,

                AlphaBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.Zero,
                DstAlphaBlendFactor = BlendFactor.One,
            },
        }.AddTo(disposables);

        copyUiPass = new CopyColorTexturePass(graphicsContext, resourceManager).AddTo(disposables);
    }

    public void Setup()
    {
        WorldColor = new Texture2D(graphicsContext, new TextureDesc2D()
        {
            Format = Format.R32G32B32A32Sfloat,
            Size = swapchain.Texture.Desc.Size,
            Usages = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.SampledBit,
        }, new TextureViewDesc()
        {
            Aspects = ImageAspectFlags.ColorBit,
        }).AddTo(disposables);

        WorldDepth = new Texture2D(graphicsContext, new TextureDesc2D()
        {
            Format = Format.D32Sfloat,
            Size = swapchain.Texture.Desc.Size,
            Usages = ImageUsageFlags.DepthStencilAttachmentBit,
        }, new TextureViewDesc()
        {
            Aspects = ImageAspectFlags.DepthBit,
        }).AddTo(disposables);
    }

    public void Render()
    {
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

        // Render world
        RenderCameraQuery(World, commandBuffer);

        // Copy world to swapchain
        copyWorldPass.Copy(commandBuffer, WorldColor, swapchain.Texture);

        // Copy UI to swapchain
        copyUiPass.Copy(commandBuffer, tetrisUiSystem.UiRoot.Texture, swapchain.Texture);
     }

    [Query]
    private void RenderCamera([Data] GraphicsCommandBuffer commandBuffer, ref ComponentCameraProjection cameraProjection)
    {
        using (var batch = spriteBatcher.Acquire(new SpriteUniformDrawSettings()
               {
                   CommandBuffer = commandBuffer,
                   ColorTarget = WorldColor,
                   DepthTarget = WorldDepth,
                   View = cameraProjection.View,
                   Projection = cameraProjection.Projection,
               }))
        {
            DrawTiles(batch);
            DrawPlaceholdersQuery(World, batch);
            DrawSpritesQuery(World, batch);
        }
    }

    private void DrawTiles(SpriteBatcher.Batch batch)
    {
        for (var x = 0; x < tilemap.Tiles.GetLength(0); x++)
        {
            for (var y = 0; y < tilemap.Tiles.GetLength(1); y++)
            {
                ref var tile = ref tilemap.Tiles[x, y];

                var texture = emptyTileTexture;
                if (tile.Shape != null)
                {
                    texture = tile.Shape.DefaultTexture;

                    if (y + 1 == tilemap.Tiles.GetLength(1) || tilemap.Tiles[x, y + 1].Shape == null)
                    {
                        texture = tile.Shape.SnowTexture;
                    }
                }

                var model = Matrix4x4.CreateTranslation(x, y, 0);

                batch.Draw(new SpriteInstanceDrawSettings()
                {
                    Texture = texture.Value,
                    Model = model,
                });
            }
        }
    }

    [Query]
    private void DrawPlaceholders([Data] SpriteBatcher.Batch batch, ref ComponentTetrisRoot tetrisRoot)
    {
        foreach (var blockPosition in tetrisRoot.PredictedBlockPositions)
        {
            var texture = placeholderTileTexture.Value;

            var maxAlpha = 0.8f;
            var minAlpha = 0.1f;
            var alpha = MathUtility.Remap(EaseInOutCubic(time.Time / 1.5f), 0, 1, minAlpha, maxAlpha);

            var model = Matrix4x4.CreateTranslation(blockPosition.X, blockPosition.Y, 0);

            batch.Draw(new SpriteInstanceDrawSettings()
            {
                Texture = texture,
                Model = model,
                Color = new Vector4(1, 1, 1, alpha),
            });
        }

        return;

        float EaseInOutCubic(float t)
        {
            t = MathUtility.Wrap(t, 0, 2);
            if (t > 1)
            {
                t = 2 - t;
            }

            return (float)(t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2);
        }
    }

     [Query]
     private void DrawSprites([Data] SpriteBatcher.Batch batch, ref ComponentSprite sprite, ref ComponentTransform transform)
     {
         var texture = sprite.Texture.Value;
         var model = Matrix4x4.CreateRotationZ(transform.Rotation) * Matrix4x4.CreateTranslation(transform.Position.X, transform.Position.Y, 0);

         batch.Draw(new SpriteInstanceDrawSettings()
         {
             Texture = texture,
             Model = model,
         });
     }

    public void Teardown()
    {
        disposables.Dispose();
    }
}

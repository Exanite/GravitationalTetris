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
using Exanite.GravitationalTetris.Features.Rendering.Passes;
using Exanite.GravitationalTetris.Features.Sprites;
using Exanite.GravitationalTetris.Features.Sprites.Components;
using Exanite.GravitationalTetris.Features.Sprites.Passes;
using Exanite.GravitationalTetris.Features.Tetris.Components;
using Exanite.GravitationalTetris.Features.Tetris.Systems;
using Exanite.GravitationalTetris.Features.Tiles;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Exanite.ResourceManagement;
using Silk.NET.Vulkan;

namespace Exanite.GravitationalTetris.Features.Rendering.Systems;

public partial class RendererSystem : GameSystem, ISetupSystem, IRenderSystem, ITeardownSystem
{
    // Array of size 2
    private Texture2D[] worldColor = null!;
    private Texture2D worldDepth = null!;

    private Texture2D ActiveWorldColor => worldColor[0];
    private Texture2D InactiveWorldColor => worldColor[1];

    private IResourceHandle<Texture2D> emptyTileTexture;
    private IResourceHandle<Texture2D> placeholderTileTexture;

    private readonly ClearPass clearPass;

    private readonly SpriteBatchPass spriteBatchPass;
    private readonly SpriteBatch spriteBatch = new();

    private readonly BloomPass bloomPass;
    private readonly ToneMapPass toneMapPass;

    private readonly CopyColorTexturePass copyWorldPass;
    private readonly CopyColorTexturePass copyUiPass;

    private DisposableCollection disposables = new();

    private readonly GraphicsContext graphicsContext;
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
        this.window = window;
        this.swapchain = swapchain;
        this.tetrisUiSystem = tetrisUiSystem;
        this.tilemap = tilemap;
        this.time = time;

        emptyTileTexture = resourceManager.GetResource(BaseMod.TileNone);
        placeholderTileTexture = resourceManager.GetResource(BaseMod.TilePlaceholder);

        clearPass = new ClearPass();

        spriteBatchPass = new SpriteBatchPass(graphicsContext, resourceManager).AddTo(disposables);

        bloomPass = new BloomPass(graphicsContext, resourceManager).AddTo(disposables);
        toneMapPass = new ToneMapPass(graphicsContext, resourceManager).AddTo(disposables);

        copyWorldPass = new CopyColorTexturePass(graphicsContext, resourceManager).AddTo(disposables);
        copyUiPass = new CopyColorTexturePass(graphicsContext, resourceManager).AddTo(disposables);
    }

    public void Setup()
    {
        worldColor = new Texture2D[2];
        for (var i = 0; i < worldColor.Length; i++)
        {
            worldColor[i] = new Texture2D(graphicsContext, new TextureDesc2D()
            {
                Format = Format.R32G32B32A32Sfloat,
                Size = swapchain.Texture.Desc.Size,
                Usages = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.SampledBit,
            }, new TextureViewDesc()
            {
                Aspects = ImageAspectFlags.ColorBit,
            }).AddTo(disposables);
        }

        worldDepth = new Texture2D(graphicsContext, new TextureDesc2D()
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
        ActiveWorldColor.ResizeIfNeeded(window.Size);
        worldDepth.ResizeIfNeeded(window.Size);

        // Clear world render targets
        clearPass.Clear(commandBuffer, [ActiveWorldColor], worldDepth);

        // Render world
        {
            // Gather data
            DrawTiles();
            DrawPlaceholdersQuery(World);
            DrawSpritesQuery(World);

            // Render
            RenderCameraQuery(World, commandBuffer);

            // Reset
            spriteBatch.Clear();
        }

        // Post process
        bloomPass.Render(commandBuffer, ActiveWorldColor);

        toneMapPass.Render(commandBuffer, ActiveWorldColor, InactiveWorldColor);
        SwapWorldColor();

        // Copy world to swapchain
        copyWorldPass.Copy(commandBuffer, ActiveWorldColor, swapchain.Texture);

        // Copy UI to swapchain
        copyUiPass.Copy(commandBuffer, tetrisUiSystem.UiRoot.Texture, swapchain.Texture);
     }

    [Query]
    private void RenderCamera([Data] GraphicsCommandBuffer commandBuffer, ref ComponentCameraProjection cameraProjection)
    {
        spriteBatchPass.Render(spriteBatch, new SpriteUniformDrawSettings()
        {
            CommandBuffer = commandBuffer,
            ColorTarget = ActiveWorldColor,
            DepthTarget = worldDepth,

            View = cameraProjection.View,
            Projection = cameraProjection.Projection,
        });
    }

    private void DrawTiles()
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

                spriteBatch.Draw(new SpriteInstanceDrawSettings()
                {
                    Texture = texture.Value,
                    Model = model,
                });
            }
        }
    }

    [Query]
    private void DrawPlaceholders(ref ComponentTetrisRoot tetrisRoot)
    {
        foreach (var blockPosition in tetrisRoot.PredictedBlockPositions)
        {
            var texture = placeholderTileTexture.Value;

            var maxAlpha = 0.8f;
            var minAlpha = 0.1f;
            var alpha = MathUtility.Remap(EaseInOutCubic(time.Time / 1.5f), 0, 1, minAlpha, maxAlpha);

            var model = Matrix4x4.CreateTranslation(blockPosition.X, blockPosition.Y, 0);

            spriteBatch.Draw(new SpriteInstanceDrawSettings()
            {
                Texture = texture,
                Model = model,
                Color = new Vector4(1, 1, 1, alpha),
            });
        }

        return;

        static float EaseInOutCubic(float t)
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
    private void DrawSprites(ref ComponentSprite sprite, ref ComponentTransform transform)
    {
        var texture = sprite.Texture.Value;
        var model = Matrix4x4.CreateRotationZ(transform.Rotation) * Matrix4x4.CreateTranslation(transform.Position.X, transform.Position.Y, 0);

        spriteBatch.Draw(new SpriteInstanceDrawSettings()
        {
            Texture = texture,
            Model = model,
        });
    }
    
    private void SwapWorldColor()
    {
        var temp = worldColor[0];
        worldColor[0] = worldColor[1];
        worldColor[1] = temp;
    }

    public void Teardown()
    {
        disposables.Dispose();
    }
}

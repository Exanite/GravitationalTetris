using Arch.System;
using Exanite.WarGames.Features.Cameras.Components;
using Exanite.WarGames.Features.Sprites.Components;
using Exanite.WarGames.Features.Transforms.Components;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exanite.WarGames.Features.Sprites.Systems;

public partial class SpriteDrawSystem : EcsSystem, IDrawSystem
{
    private readonly GameSpriteBatch gameSpriteBatch;

    public SpriteDrawSystem(GameSpriteBatch gameSpriteBatch)
    {
        this.gameSpriteBatch = gameSpriteBatch;
    }

    public void Draw()
    {
        DrawQuery(World);
    }

    [Query]
    private void Draw(ref CameraComponent camera, ref CameraProjectionComponent cameraProjection)
    {
        var spriteBatch = gameSpriteBatch.SpriteBatch;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            cameraProjection.WorldToScreen
        );
        {
            DrawSpritesQuery(World, spriteBatch);
        }
        spriteBatch.End();
    }

    [Query]
    private void DrawSprites([Data] SpriteBatch spriteBatch, ref SpriteComponent sprite, ref TransformComponent transform)
    {
        var texture = sprite.Resource.Value;
        if (texture == null)
        {
            return;
        }

        var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
        var origin = new Vector2(0.5f * texture.Width, 0.5f * texture.Height);
        var scale = new Vector2(1f / texture.Width, 1f / texture.Height) * transform.Size;

        spriteBatch.Draw(
            texture, // Texture
            transform.Position, // Position
            sourceRect, // Source Rect
            Color.White, // Color
            transform.Rotation, // Rotation
            origin, // Origin
            scale, // Scale
            SpriteEffects.None, // Effects
            0); // Depth
    }
}

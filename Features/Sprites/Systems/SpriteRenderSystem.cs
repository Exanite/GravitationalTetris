using System.Numerics;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Ecs.Systems;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Rendering;
using Exanite.GravitationalTetris.Features.Sprites.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;

namespace Exanite.GravitationalTetris.Features.Sprites.Systems;

public partial class SpriteRenderSystem : EcsSystem, IRenderSystem
{
    private readonly SpriteBatchSystem spriteBatchSystem;

    public SpriteRenderSystem(SpriteBatchSystem spriteBatchSystem)
    {
        this.spriteBatchSystem = spriteBatchSystem;
    }

    public void Render()
    {
        DrawQuery(World);
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
        var texture = sprite.Texture.Value;

        var world = Matrix4x4.CreateRotationZ(transform.Rotation) * Matrix4x4.CreateTranslation(transform.Position.X, transform.Position.Y, 0);
        var view = cameraProjection.View;
        var projection = cameraProjection.Projection;

        spriteBatchSystem.DrawSprite(texture, new SpriteUniformData
        {
            World = world,
            View = view,
            Projection = projection,
            Color = Vector4.One,
        });
    }
}

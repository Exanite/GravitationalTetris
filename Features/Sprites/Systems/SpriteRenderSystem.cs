using System.Numerics;
using Exanite.Ecs.Systems;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Sprites.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Flecs.NET.Core;

namespace Exanite.GravitationalTetris.Features.Sprites.Systems;

public class SpriteRenderSystem : EcsSystem, ISetupSystem, IRenderSystem
{
    private Query renderToCameraQuery;

    private readonly SpriteBatchSystem spriteBatchSystem;

    public SpriteRenderSystem(SpriteBatchSystem spriteBatchSystem)
    {
        this.spriteBatchSystem = spriteBatchSystem;
    }

    public void Setup()
    {
        renderToCameraQuery = World.Query(World.FilterBuilder<CameraComponent, CameraProjectionComponent>());
    }

    public void Render()
    {
        renderToCameraQuery.Each<CameraProjectionComponent>(RenderToCamera);
    }

    private void RenderToCamera(ref CameraProjectionComponent cameraProjection)
    {
        spriteBatchSystem.Begin(new SpriteBeginDrawOptions
        {
            View = cameraProjection.View,
            Projection = cameraProjection.Projection,
        });
        {
            World.Each<SpriteComponent, TransformComponent>(DrawSprites);
        }
        spriteBatchSystem.End();
    }

    private void DrawSprites(ref SpriteComponent sprite, ref TransformComponent transform)
    {
        var texture = sprite.Texture.Value;
        var world = Matrix4x4.CreateRotationZ(transform.Rotation) * Matrix4x4.CreateTranslation(transform.Position.X, transform.Position.Y, 0);

        spriteBatchSystem.Draw(new SpriteDrawOptions
        {
            Texture = texture,
            World = world,
        });
    }
}

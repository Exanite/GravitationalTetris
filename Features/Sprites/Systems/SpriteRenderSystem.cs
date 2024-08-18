using System.Numerics;
using Exanite.Engine.Ecs.Queries;
using Exanite.Engine.Ecs.Systems;
using Exanite.GravitationalTetris.Features.Cameras.Components;
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
    [Include<ComponentCamera>]
    private void Draw(ref ComponentCameraProjection cameraProjection)
    {
        spriteBatchSystem.Begin(new SpriteBeginDrawOptions()
        {
            View = cameraProjection.View,
            Projection = cameraProjection.Projection,
        });
        {
            DrawSpritesQuery(World);
        }
        spriteBatchSystem.End();
    }

    [Query]
    private void DrawSprites(ref ComponentSprite sprite, ref ComponentTransform transform)
    {
        var texture = sprite.Texture.Value;
        var world = Matrix4x4.CreateRotationZ(transform.Rotation) * Matrix4x4.CreateTranslation(transform.Position.X, transform.Position.Y, 0);

        spriteBatchSystem.Draw(new SpriteDrawOptions()
        {
            Texture = texture,
            World = world,
        });
    }
}

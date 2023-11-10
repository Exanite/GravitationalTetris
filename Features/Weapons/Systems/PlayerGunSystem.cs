using System;
using Arch.System;
using Arch.System.SourceGenerator;
using Exanite.Extraction.Features.Players;
using Exanite.Extraction.Features.Players.Components;
using Exanite.Extraction.Features.Resources;
using Exanite.Extraction.Features.Sprites.Components;
using Exanite.Extraction.Features.Time;
using Exanite.Extraction.Features.Transforms.Components;
using Exanite.Extraction.Features.Weapons.Components;
using Exanite.Extraction.Systems;
using Exanite.ResourceManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using World = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.Extraction.Features.Weapons.Systems;

public partial class PlayerGunSystem : EcsSystem, IUpdateSystem
{
    private readonly GameInputData input;
    private readonly GameTimeData time;
    private readonly World physicsWorld;
    private readonly ResourceManager resourceManager;

    public PlayerGunSystem(GameInputData input, GameTimeData time, World physicsWorld, ResourceManager resourceManager)
    {
        this.input = input;
        this.time = time;
        this.physicsWorld = physicsWorld;
        this.resourceManager = resourceManager;
    }

    public void Update()
    {
        UpdateGunCooldownsQuery(World);
        FirePlayerGunQuery(World);
    }

    [Query]
    private void UpdateGunCooldowns(ref GunComponent gun)
    {
        gun.CurrentCooldown -= time.DeltaTime;
    }

    [Query]
    [All<PlayerComponent>]
    private void FirePlayerGun(ref TransformComponent transform, ref GunComponent gun)
    {
        if (input.Current.Mouse.LeftButton == ButtonState.Pressed && gun.CurrentCooldown < 0)
        {
            gun.CurrentCooldown = 1f / gun.RoundsPerSecond;

            var direction = new Vector2(MathF.Cos(transform.Rotation), MathF.Sin(transform.Rotation));

            var hitPoint = transform.Position + direction * gun.Range;
            physicsWorld.RayCast((fixture, point, normal, fraction) =>
            {
                hitPoint = point;

                return fraction;
            }, transform.Position, hitPoint);

            World.Create(
                new TransformComponent
                {
                    Position = (transform.Position + hitPoint) / 2,
                    Rotation = transform.Rotation,
                    Size = new Vector2((hitPoint - transform.Position).Length(), 0.1f),
                },
                new RaycastBulletComponent(),
                new SpriteComponent(resourceManager.GetResource(Base.Wall)));
        }
    }
}

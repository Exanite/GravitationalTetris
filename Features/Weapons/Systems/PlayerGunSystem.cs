using System;
using Arch.System;
using Exanite.ResourceManagement;
using Exanite.WarGames.Features.Players;
using Exanite.WarGames.Features.Players.Components;
using Exanite.WarGames.Features.Resources;
using Exanite.WarGames.Features.Sprites.Components;
using Exanite.WarGames.Features.Time;
using Exanite.WarGames.Features.Transforms.Components;
using Exanite.WarGames.Features.Weapons.Components;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using World = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.WarGames.Features.Weapons.Systems;

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
                new SpriteComponent(resourceManager.GetResource<>(Base.Wall)));
        }
    }
}

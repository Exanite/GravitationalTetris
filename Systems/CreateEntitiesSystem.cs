using Exanite.ResourceManagement;
using Exanite.WarGames.Features.Cameras.Components;
using Exanite.WarGames.Features.Characters.Components;
using Exanite.WarGames.Features.Enemies.Components;
using Exanite.WarGames.Features.Physics.Components;
using Exanite.WarGames.Features.Players.Components;
using Exanite.WarGames.Features.Resources;
using Exanite.WarGames.Features.Sprites.Components;
using Exanite.WarGames.Features.Transforms.Components;
using Exanite.WarGames.Features.Weapons.Components;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.WarGames.Systems;

public class CreateEntitiesSystem : EcsSystem, IStartSystem
{
    private readonly ResourceManager resourceManager;

    public CreateEntitiesSystem(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;
    }

    public void Start()
    {
        // Camera
        World.Create(
            new CameraComponent(12),
            new TransformComponent(),
            new CameraProjectionComponent());

        // Player
        var playerBody = new Body();
        playerBody.CreateCircle(0.5f, 1);
        playerBody.BodyType = BodyType.Dynamic;
        playerBody.LinearDamping = 5;
        playerBody.AngularDamping = 5;

        World.Create(
            new PlayerComponent(),
            new TransformComponent()
            {
                Position = new Vector2(1, 1),
            },
            new CameraTargetComponent(),
            new SpriteComponent(resourceManager.GetResource<>(Base.Player)),
            new RigidbodyComponent(playerBody),
            new VelocityComponent(),
            new SmoothDampMovementComponent(0.05f),
            new MovementSpeedComponent(5),
            new MovementDirectionComponent(),
            new GunComponent()
            {
                Range = 10,
                RoundsPerSecond = 10,
            });

        // Enemies
        for (var x = 0; x < 10; x++)
        {
            for (var y = 0; y < 10; y++)
            {
                var body = new Body();
                body.CreateCircle(0.5f, 1);
                body.BodyType = BodyType.Dynamic;
                body.LinearDamping = 5;
                body.AngularDamping = 5;

                World.Create(
                    new EnemyComponent(),
                    new SpriteComponent(resourceManager.GetResource<>(Base.Player)),
                    new RigidbodyComponent(body),
                    new TransformComponent
                    {
                        Position = new Vector2(x + 10, y + 5),
                    },
                    new VelocityComponent(),
                    new SmoothDampMovementComponent(0.05f),
                    new MovementSpeedComponent(1),
                    new MovementDirectionComponent());
            }
        }

        // Projectiles
        for (var i = 0; i < 10; i++)
        {
            var body = new Body();
            body.CreateCircle(0.05f, 1);
            body.BodyType = BodyType.Dynamic;
            body.LinearDamping = 5;
            body.AngularDamping = 5;

            World.Create(
                new SpriteComponent(resourceManager.GetResource<>(Base.Player)),
                new RigidbodyComponent(body),
                new TransformComponent
                {
                    Position = new Vector2(-1, -2 - i * 0.25f),
                    Size = new Vector2(0.1f, 0.1f),
                },
                new VelocityComponent(new Vector2(-i * 5, 0)));
        }
    }
}

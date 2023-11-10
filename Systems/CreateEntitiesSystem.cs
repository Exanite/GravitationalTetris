using Exanite.ResourceManagement;
using Exanite.WarGames.Features.Cameras.Components;
using Exanite.WarGames.Features.Characters.Components;
using Exanite.WarGames.Features.Physics.Components;
using Exanite.WarGames.Features.Players.Components;
using Exanite.WarGames.Features.Resources;
using Exanite.WarGames.Features.Sprites.Components;
using Exanite.WarGames.Features.Transforms.Components;
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
            new SpriteComponent(resourceManager.GetResource(Base.White)),
            new RigidbodyComponent(playerBody),
            new VelocityComponent(),
            new SmoothDampMovementComponent(0.05f),
            new MovementSpeedComponent(5),
            new MovementDirectionComponent());
    }
}

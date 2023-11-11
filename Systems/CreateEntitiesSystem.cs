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
            new CameraComponent(20),
            new TransformComponent()
            {
                Position = new Vector2(5f - 0.5f, 10f - 0.5f),
            },
            new CameraProjectionComponent());

        // Player
        var playerBody = new Body();
        playerBody.FixedRotation = true;
        playerBody.BodyType = BodyType.Dynamic;

        var head = playerBody.CreateRectangle(8f / 16f, 5f / 16f, 1, new Vector2(0, -0.5f / 16f));
        head.Friction = 0;

        var body = playerBody.CreateRectangle(4f / 16f, 10f / 16f, 1, new Vector2(0, 2f / 16f));
        body.Friction = 0;

        playerBody.CreateCircle(2f / 16f, 1, new Vector2(0, 6f / 16f));

        World.Create(
            new PlayerComponent(),
            new TransformComponent()
            {
                Position = new Vector2(-3, 0),
                Size = new Vector2(1, 1),
            },
            new CameraTargetComponent(),
            new SpriteComponent(resourceManager.GetResource(Base.Player)),
            new RigidbodyComponent(playerBody),
            new VelocityComponent(),
            new MovementSpeedComponent(5),
            new PlayerMovement()
            {
                SmoothTime = 0.05f,
            });
    }
}

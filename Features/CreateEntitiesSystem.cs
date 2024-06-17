using System.Numerics;
using Exanite.Engine.Ecs.Systems;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Players.Components;
using Exanite.GravitationalTetris.Features.Sprites.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Exanite.ResourceManagement;
using Myriad.ECS.Command;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.GravitationalTetris.Features;

public class CreateEntitiesSystem : EcsSystem, IStartSystem
{
    private readonly ResourceManager resourceManager;

    public CreateEntitiesSystem(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;
    }

    public void Start()
    {
        var commandBuffer = new CommandBuffer(World);

        // Camera
        commandBuffer.Create()
            .Set(new CameraComponent(20))
            .Set(new TransformComponent
            {
                Position = new Vector2(5f - 0.5f, 10f - 0.5f),
            })
            .Set(new CameraProjectionComponent());

        // Player
        var playerBody = new Body();
        playerBody.FixedRotation = true;
        playerBody.BodyType = BodyType.Dynamic;
        playerBody.IsBullet = true;
        playerBody.SleepingAllowed = false;

        var head = playerBody.CreateRectangle(8f / 16f, 5f / 16f, 1, new Vector2(0, -0.5f / 16f));
        head.Friction = 0;
        head.Restitution = 0;

        var body = playerBody.CreateRectangle(4f / 16f, 10f / 16f, 1, new Vector2(0, 2f / 16f));
        body.Friction = 0;
        body.Restitution = 0;

        var feet = playerBody.CreateCircle(2f / 16f, 1, new Vector2(0, 6f / 16f));
        feet.Restitution = 0;

        commandBuffer.Create()
            .Set(new PlayerComponent())
            .Set(new TransformComponent
            {
                Position = new Vector2(4f, 0),
                Size = new Vector2(1, 1),
            })
            .Set(new SpriteComponent(resourceManager.GetResource(BaseMod.Player)))
            .Set(new RigidbodyComponent(playerBody))
            .Set(new VelocityComponent())
            .Set(new MovementSpeedComponent(5))
            .Set(new PlayerMovement
            {
                SmoothTime = 0.05f,
            });

        // Walls on side
        var wallBody = new Body();
        wallBody.BodyType = BodyType.Static;

        wallBody.CreateRectangle(1, 60, 1, new Vector2(-2, 15));
        wallBody.CreateRectangle(1, 60, 1, new Vector2(11, 15));

        commandBuffer.Create().Set(new RigidbodyComponent(wallBody));

        commandBuffer.Playback().Dispose();
    }
}

using System.Numerics;
using Exanite.Engine.Ecs.Components;
using Exanite.Engine.Ecs.Systems;
using Exanite.Engine.Sprites.Components;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Physics.Components;
using Exanite.GravitationalTetris.Features.Players.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.ResourceManagement;
using nkast.Aether.Physics2D.Dynamics;

namespace Exanite.GravitationalTetris.Features;

public class CreateEntitiesSystem : EngineSystem, IStartSystem
{
    private readonly ResourceManager resourceManager;
    private readonly EcsCommandBuffer commandBuffer;

    public CreateEntitiesSystem(ResourceManager resourceManager, EcsCommandBuffer commandBuffer)
    {
        this.resourceManager = resourceManager;
        this.commandBuffer = commandBuffer;
    }

    public void Start()
    {
        // Camera
        commandBuffer.Create()
            .Set(new EcsName("Camera"))
            .Set(new EcsCamera(20))
            .Set(new EcsTransform
            {
                Position = new Vector2(5f - 0.5f, 10f - 0.5f),
            })
            .Set(new EcsCameraProjection());

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
            .Set(new EcsName("Player"))
            .Set(new EcsPlayer())
            .Set(new EcsTransform
            {
                Position = new Vector2(4f, 0),
                Size = new Vector2(1, 1),
            })
            .Set(new EcsSprite(GravitationalTetrisConstants.IsWinter
                ? resourceManager.GetResource(GravitationalTetrisResources.ExaniteGravitationalTetrisWinter.PlayerTexture)
                : resourceManager.GetResource(GravitationalTetrisResources.ExaniteGravitationalTetris.PlayerTexture)))
            .Set(new EcsRigidbody(playerBody))
            .Set(new EcsVelocity())
            .Set(new EcsMovementSpeed(5))
            .Set(new EcsPlayerMovement
            {
                SmoothTime = 0.05f,
            });

        // Walls on side
        var wallBody = new Body();
        wallBody.BodyType = BodyType.Static;

        wallBody.CreateRectangle(1, 60, 1, new Vector2(-2, 15));
        wallBody.CreateRectangle(1, 60, 1, new Vector2(11, 15));

        commandBuffer.Create()
            .Set(new EcsName("Wall"))
            .Set(new EcsRigidbody(wallBody));

        commandBuffer.Execute();
    }
}

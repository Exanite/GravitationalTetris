using System.Numerics;
using Arch.System;
using Arch.System.SourceGenerator;
using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.GravitationalTetris.Features.Cameras.Components;
using Exanite.GravitationalTetris.Features.Rendering;
using Exanite.GravitationalTetris.Features.Sprites.Components;
using Exanite.GravitationalTetris.Features.Transforms.Components;
using ValueType = Diligent.ValueType;

namespace Exanite.GravitationalTetris.Features.Sprites.Systems;

public partial class SpriteRenderSystem : EcsSystem, IRenderSystem
{
    private readonly RendererContext rendererContext;
    private readonly RenderingResourcesSystem renderingResourcesSystem;
    private readonly SpriteBatchSystem spriteBatchSystem;

    public SpriteRenderSystem(RendererContext rendererContext, RenderingResourcesSystem renderingResourcesSystem, SpriteBatchSystem spriteBatchSystem)
    {
        this.rendererContext = rendererContext;
        this.renderingResourcesSystem = renderingResourcesSystem;
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
        var deviceContext = rendererContext.DeviceContext;
        var shaderResourceBinding = renderingResourcesSystem.ShaderResourceBinding;
        var uniformBuffer = renderingResourcesSystem.UniformBuffer;
        var pipeline = renderingResourcesSystem.Pipeline;
        var mesh = renderingResourcesSystem.Mesh;

        var texture = sprite.Texture.Value;
        shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture").Set(texture.View, SetShaderResourceFlags.AllowOverwrite);

        var world = Matrix4x4.CreateRotationZ(transform.Rotation) * Matrix4x4.CreateTranslation(transform.Position.X, transform.Position.Y, 0);
        var view = cameraProjection.View;
        var projection = cameraProjection.Projection;

        var mapUniformBuffer = uniformBuffer.Map(MapType.Write, MapFlags.Discard);
        {
            mapUniformBuffer[0].World = world;
            mapUniformBuffer[0].View = view;
            mapUniformBuffer[0].Projection = projection;
            mapUniformBuffer[0].Color = Vector4.One;
        }
        uniformBuffer.Unmap(MapType.Write);

        deviceContext.SetPipelineState(pipeline);
        deviceContext.SetVertexBuffers(0, new[] { mesh.VertexBuffer }, new[] { 0ul }, ResourceStateTransitionMode.Transition);
        deviceContext.SetIndexBuffer(mesh.IndexBuffer, 0, ResourceStateTransitionMode.Transition);
        deviceContext.CommitShaderResources(shaderResourceBinding, ResourceStateTransitionMode.Transition);
        deviceContext.DrawIndexed(new DrawIndexedAttribs
        {
            IndexType = ValueType.UInt32,
            NumIndices = 36,
            Flags = DrawFlags.VerifyAll,
        });
    }
}

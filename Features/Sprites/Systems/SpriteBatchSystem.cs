using Diligent;
using Exanite.Ecs.Systems;
using Exanite.Engine.Rendering;
using Exanite.Engine.Rendering.Systems;
using Exanite.GravitationalTetris.Features.Rendering;

namespace Exanite.GravitationalTetris.Features.Sprites.Systems;

public class SpriteBatchSystem : ISystem
{
    private readonly RendererContext rendererContext;
    private readonly RenderingResourcesSystem renderingResourcesSystem;

    public SpriteBatchSystem(RendererContext rendererContext, ClearRenderTargetRenderSystem clearRenderTargetRenderSystem, RenderingResourcesSystem renderingResourcesSystem)
    {
        this.rendererContext = rendererContext;
        this.renderingResourcesSystem = renderingResourcesSystem;
    }

    public void DrawSprite(Texture2D texture, SpriteUniformData spriteUniformData)
    {
        var deviceContext = rendererContext.DeviceContext;
        var shaderResourceBinding = renderingResourcesSystem.ShaderResourceBinding;
        var uniformBuffer = renderingResourcesSystem.UniformBuffer;
        var pipeline = renderingResourcesSystem.Pipeline;
        var mesh = renderingResourcesSystem.Mesh;

        shaderResourceBinding.GetVariableByName(ShaderType.Pixel, "Texture").Set(texture.View, SetShaderResourceFlags.AllowOverwrite);

        var mapUniformBuffer = uniformBuffer.Map(MapType.Write, MapFlags.Discard);
        {
            mapUniformBuffer[0] = spriteUniformData;
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

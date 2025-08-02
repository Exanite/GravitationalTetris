using Autofac;
using Exanite.Engine.Modding;

namespace Exanite.GravitationalTetris.Features.Sprites.Modules;

public class SpriteRenderingModule : GameModule
{
    protected override void OnConfigureContainer(ContainerBuilder builder)
    {
        base.OnConfigureContainer(builder);

        builder.RegisterType<SpriteBatcher>().SingleInstance();
    }
}

using Autofac;

namespace Exanite.GravitationalTetris.Features.Ui;

public class UiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ExaniteEngineMyraPlatform>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ExaniteEngineMyraRenderer>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ExaniteEngineFontTextureManager>().AsSelf().AsImplementedInterfaces().SingleInstance();
    }
}

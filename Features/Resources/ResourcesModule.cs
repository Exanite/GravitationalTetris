using System.IO;
using Autofac;
using Exanite.Engine.Rendering;
using Exanite.Engine.Resources.Loaders;
using Exanite.ResourceManagement;
using Exanite.ResourceManagement.FileSystems;
using Serilog;

namespace Exanite.GravitationalTetris.Features.Resources;

public class ResourcesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.Register(ctx =>
            {
                var logger = ctx.Resolve<ILogger>();

                return new ResourceManager(new ResourceManagerSettings
                {
                    Logger = logger,
                    EnableHotReload = true,
                });
            })
            .SingleInstance()
            .AsSelf()
            .AsImplementedInterfaces()
            .OnActivating(e =>
            {
                var resourceManager = e.Instance;
                var rendererContext = e.Context.Resolve<RendererContext>();

                resourceManager.Mount("/Base/", new DirectoryFileSystem(Path.Join(GameDirectories.ContentDirectory, "Base")), true);

                resourceManager.Mount("/Winter/", new DirectoryFileSystem(Path.Join(GameDirectories.ContentDirectory, "Winter", "Content")), true);
                resourceManager.Mount("/Base/", new DirectoryFileSystem(Path.Join(GameDirectories.ContentDirectory, "Winter", "Overrides", "Base")), true);

                resourceManager.Mount("/Rendering/", new DirectoryFileSystem(Path.Join(GameDirectories.ContentDirectory, "Rendering")), true);

                resourceManager.RegisterLoader(new ShaderLoader(rendererContext));
                resourceManager.RegisterLoader(new Texture2DLoader(rendererContext));
            });
    }
}

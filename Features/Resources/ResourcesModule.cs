using System.IO;
using Autofac;
using Exanite.Engine.EngineUsage;
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
                    HotReloadSupport = true,
                });
            })
            .SingleInstance()
            .AsSelf()
            .AsImplementedInterfaces()
            .OnActivating(e =>
            {
                var resourceManager = e.Instance;
                var rendererContext = e.Context.Resolve<RendererContext>();
                var paths = e.Context.Resolve<EnginePaths>();

                resourceManager.Mount("/Base/", new FolderFileSystem(Path.Join(paths.ContentFolder, "Base")), true);

                resourceManager.Mount("/Winter/", new FolderFileSystem(Path.Join(paths.ContentFolder, "Winter", "Content")), true);
                resourceManager.Mount("/Base/", new FolderFileSystem(Path.Join(paths.ContentFolder, "Winter", "Overrides", "Base")), true);

                resourceManager.Mount("/Rendering/", new FolderFileSystem(Path.Join(paths.ContentFolder, "Rendering")), true);

                resourceManager.RegisterLoader(new ShaderLoader(rendererContext));
                resourceManager.RegisterLoader(new Texture2DLoader(rendererContext));
            });
    }
}

using System;
using System.IO;
using Autofac;
using Diligent;
using Exanite.Engine.Rendering;
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
                    EnableImmediateHotReload = true,
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

                resourceManager.RegisterLoader<Shader>(loadOperation =>
                {
                    using var stream = loadOperation.Open(loadOperation.Key);
                    using var reader = new StreamReader(stream);

                    ShaderType type;
                    if (loadOperation.Key.EndsWith(".v.hlsl"))
                    {
                        type = ShaderType.Vertex;
                    }
                    else if (loadOperation.Key.EndsWith(".p.hlsl"))
                    {
                        type = ShaderType.Pixel;
                    }
                    else
                    {
                        throw new NotSupportedException($"Failed to load {loadOperation.Key} as a shader. The key does not end in a valid extension.");
                    }

                    var shader = new Shader(reader.ReadToEnd(), type, rendererContext);
                    loadOperation.Fulfill(shader);
                });

                resourceManager.RegisterLoader<Texture2D>(loadOperation =>
                {
                    using var stream = loadOperation.Open(loadOperation.Key);

                    var texture = new Texture2D(rendererContext, stream);
                    loadOperation.Fulfill(texture);
                });
            });
    }
}

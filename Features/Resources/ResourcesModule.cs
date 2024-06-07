using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autofac;
using Diligent;
using Exanite.Engine.Rendering;
using Exanite.ResourceManagement;
using Exanite.ResourceManagement.FileSystems;
using FontStashSharp;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI.Styles;
using Myra.Utility;
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

                resourceManager.Mount("Base:", new DirectoryFileSystem(Path.Join(GameDirectories.ContentDirectory, "Base")), true);

                resourceManager.Mount("Myra:", new DirectoryFileSystem(Path.Join(GameDirectories.ContentDirectory, "Myra")), true);

                resourceManager.Mount("Winter:", new DirectoryFileSystem(Path.Join(GameDirectories.ContentDirectory, "Winter", "Content")), true);
                resourceManager.Mount("Base:", new DirectoryFileSystem(Path.Join(GameDirectories.ContentDirectory, "Winter", "Overrides", "Base")), true);

                resourceManager.Mount("Rendering:", new DirectoryFileSystem(Path.Join(GameDirectories.ContentDirectory, "Rendering")), true);

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

                    var shader = new Shader(loadOperation.Key, reader.ReadToEnd(), type, rendererContext);
                    loadOperation.Fulfill(shader);
                }, false);

                resourceManager.RegisterLoader<Texture2D>(loadOperation =>
                {
                    using var stream = loadOperation.Open(loadOperation.Key);

                    var texture = new Texture2D(rendererContext, loadOperation.Key, stream);
                    loadOperation.Fulfill(texture);
                });

                // Myra
                resourceManager.RegisterLoader<TextureRegionAtlas>(loadOperation =>
                {
                    using var stream = loadOperation.Open(loadOperation.Key);
                    using var streamReader = new StreamReader(stream);
                    var data = streamReader.ReadToEnd();

                    loadOperation.Fulfill(TextureRegionAtlas.Load(data, name => resourceManager.GetResource<Texture2D>(name).Value));
                });

                resourceManager.RegisterLoader<StaticSpriteFont>(loadOperation =>
                {
                    using var stream = loadOperation.Open(loadOperation.Key);
                    using var streamReader = new StreamReader(stream);
                    var fontData = streamReader.ReadToEnd();

                    loadOperation.Fulfill(StaticSpriteFont.FromBMFont(fontData,
                        name =>
                        {
                            var region = resourceManager.GetResource<TextureRegion>(name).Value;

                            return new TextureWithOffset(region.Texture, region.Bounds.Location);
                        }));
                });

                // Original docs:
                // Loads a font by either ttf name/size(i.e. 'font.ttf:32') or by fnt
                // name(i.e. 'font.fnt')
                resourceManager.RegisterLoader<SpriteFontBase>(loadOperation =>
                {
                    if (loadOperation.Key.Contains(".fnt"))
                    {
                        loadOperation.Fulfill(resourceManager.GetResource<StaticSpriteFont>(loadOperation.Key).Value);

                        return;
                    }

                    if (loadOperation.Key.Contains(".ttf"))
                    {
                        var parts = loadOperation.Key.Split(':');
                        if (parts.Length < 2)
                        {
                            throw new Exception("Missing font size");
                        }

                        var fontSize = int.Parse(parts[1].Trim());
                        var fontSystem = resourceManager.GetResource<FontSystem>(parts[0].Trim()).Value;

                        loadOperation.Fulfill(fontSystem.GetFont(fontSize));
                    }

                    throw new ResourceLoadException(loadOperation.Key, "Failed to load font.");
                });

                resourceManager.RegisterLoader<Stylesheet>(loadOperation =>
                {
                    using var stream = loadOperation.Open(loadOperation.Key);
                    using var streamReader = new StreamReader(stream);
                    var xml = streamReader.ReadToEnd();

                    var xDoc = XDocument.Parse(xml);
                    var attr = xDoc.Root!.Attribute("TextureRegionAtlas");
                    if (attr == null)
                    {
                        throw new Exception("Mandatory attribute 'TextureRegionAtlas' doesnt exist");
                    }

                    var textureRegionAtlas = resourceManager.GetResource<TextureRegionAtlas>(attr.Value).Value;

                    // Load fonts
                    var fonts = new Dictionary<string, SpriteFontBase>();
                    var fontsNode = xDoc.Root.Element("Fonts")!;

                    var usedSpaceAttr = fontsNode.Attribute("UsedSpace");
                    Texture2D? existingTexture = null;
                    var existingTextureUsedSpace = Rectangle.Empty;
                    if (usedSpaceAttr != null)
                    {
                        var usedSpace = usedSpaceAttr.Value.ParseRectangle();

                        existingTexture = (Texture2D)textureRegionAtlas.Texture;
                        existingTextureUsedSpace = usedSpace;
                    }

                    foreach (var el in fontsNode.Elements())
                    {
                        SpriteFontBase? font;

                        var fontFile = el.Attribute("File")!.Value;
                        if (fontFile.EndsWith(".ttf") || fontFile.EndsWith(".otf"))
                        {
                            // Todo Research why this is here and why it is unused
                            var parts = new List<string>
                            {
                                fontFile,
                            };

                            var typeAttribute = el.Attribute("Effect");
                            if (typeAttribute != null)
                            {
                                parts.Add(typeAttribute.Value);

                                var amountAttribute = el.Attribute("Amount");
                                parts.Add(amountAttribute!.Value);
                            }

                            if (el.Attribute("Size") == null)
                            {
                                throw new Exception($"Can't load stylesheet ttf font '{fontFile}', since Size isn't specified.");
                            }

                            parts.Add(el.Attribute("Size")!.Value);

                            var loadSettings = new FontSystemLoadingSettings
                            {
                                ExistingTexture = existingTexture,
                                ExistingTextureUsedSpace = existingTextureUsedSpace,
                            };
                            var fontSystem = resourceManager.GetResource<FontSystem>(fontFile, loadSettings).Value;

                            font = fontSystem.GetFont(float.Parse(el.Attribute("Size")!.Value));
                        }
                        else if (fontFile.EndsWith(".fnt"))
                        {
                            font = resourceManager.GetResource<StaticSpriteFont>(fontFile).Value;
                        }
                        else
                        {
                            throw new Exception(string.Format("Font '{0}' isn't supported", fontFile));
                        }

                        fonts[el.Attribute("Id")!.Value] = font;
                    }

                    loadOperation.Fulfill(Stylesheet.LoadFromSource(xml, textureRegionAtlas, fonts));
                });

                // Original docs:
                // Loads texture region by either image name(i.e. 'image.png') or atlas
                // name/id(i.e. 'atlas.xmat:id')
                resourceManager.RegisterLoader<TextureRegion>(loadOperation =>
                {
                    if (loadOperation.Key.Contains(":"))
                    {
                        // First part is texture region atlas name
                        // Second part is texture region name
                        var parts = loadOperation.Key.Split(':');
                        var textureRegionAtlas = resourceManager.GetResource<TextureRegionAtlas>(parts[0]).Value;

                        loadOperation.Fulfill(textureRegionAtlas[parts[1]]);

                        return;
                    }

                    // Ordinary texture
                    var texture = resourceManager.GetResource<Texture2D>(loadOperation.Key).Value;
                    loadOperation.Fulfill(new TextureRegion(texture, new Rectangle(0, 0, texture.Width, texture.Height)));
                });

                resourceManager.RegisterLoader<FontSystem>(loadOperation =>
                {
                    var fontSystemSettings = new FontSystemSettings();
                    var fontSystemLoadingSettings = loadOperation.LoaderSettings as FontSystemLoadingSettings;
                    if (fontSystemLoadingSettings != null)
                    {
                        fontSystemSettings.ExistingTexture = fontSystemLoadingSettings.ExistingTexture;
                        fontSystemSettings.ExistingTextureUsedSpace = fontSystemLoadingSettings.ExistingTextureUsedSpace;
                    }

                    using var stream = loadOperation.Open(loadOperation.Key);
                    var fontSystem = new FontSystem(fontSystemSettings);
                    fontSystem.AddFont(stream);

                    if (fontSystemLoadingSettings != null && fontSystemLoadingSettings.AdditionalFonts != null)
                    {
                        foreach (var file in fontSystemLoadingSettings.AdditionalFonts)
                        {
                            using var stream2 = loadOperation.Open(file);
                            fontSystem.AddFont(stream2);
                        }
                    }

                    loadOperation.Fulfill(fontSystem);
                });
            });
    }
}

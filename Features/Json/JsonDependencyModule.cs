using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Module = Autofac.Module;
using VersionConverter = Newtonsoft.Json.Converters.VersionConverter;

namespace Exanite.WarGames.Features.Json;

public class JsonDependencyModule : Module
{
    private const string ApplySerializerSettingsMethodName = "ApplySerializerSettings";

    protected override void Load(ContainerBuilder builder)
    {
        RegisterConverters(builder);

        builder.Register(CreateJsonSerializerSettings).SingleInstance();
        builder.RegisterType<DefaultContractResolver>().AsSelf().As<IContractResolver>().SingleInstance();
        builder.RegisterType<ProjectJsonSerializer>().AsSelf().AsImplementedInterfaces().OnActivating(ApplySerializerSettings);
    }

    private void RegisterConverters(ContainerBuilder builder)
    {
        // Default
        BindConverter<VersionConverter>(builder);
        BindConverter<StringEnumConverter>(builder);
    }

    private void BindConverter<T>(ContainerBuilder builder) where T : JsonConverter
    {
        builder.RegisterType<T>().As<JsonConverter>().AsSelf().SingleInstance();
    }

    private JsonSerializerSettings CreateJsonSerializerSettings(IComponentContext context)
    {
        return new JsonSerializerSettings
        {
            Converters = context.Resolve<IList<JsonConverter>>(),
            ContractResolver = context.Resolve<IContractResolver>(),
        };
    }

    private void ApplySerializerSettings(IActivatingEventArgs<ProjectJsonSerializer> args)
    {
        var settings = args.Context.Resolve<JsonSerializerSettings>();
        var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
        var applySettingsMethodInfo = typeof(JsonSerializer).GetMethod(ApplySerializerSettingsMethodName, flags)!;

        applySettingsMethodInfo.Invoke(null, new object[] { args.Instance, settings });
    }
}

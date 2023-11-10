using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exanite.WarGames.Features.Json;

public class ProjectJsonSerializer : JsonSerializer
{
    public string Serialize(object? value)
    {
        using var stringWriter = new StringWriter();
        using var jsonWriter = new JsonTextWriter(stringWriter);

        Serialize(jsonWriter, value);

        return stringWriter.ToString();
    }

    public T? Deserialize<T>(string json)
    {
        using var textReader = new StringReader(json);
        using var jsonReader = new JsonTextReader(textReader);

        return Deserialize<T>(jsonReader);
    }

    public T? Deserialize<T>(JToken token)
    {
        using (var reader = token.CreateReader())
        {
            return Deserialize<T>(reader);
        }
    }

    [return: NotNullIfNotNull("value")]
    public T? Clone<T>(T? value)
    {
        if (value == null)
        {
            return default;
        }

        using (var stringReader = new StringReader(Serialize(value)))
        using (var textReader = new JsonTextReader(stringReader))
        {
            return (T)Deserialize(textReader, value.GetType())!;
        }
    }

    public T? PopulateWithConverter<T>(JToken token, T? value, bool requireConverter = true)
    {
        using (var reader = token.CreateReader())
        {
            return (T?)PopulateWithConverter(reader, value, typeof(T), requireConverter);
        }
    }

    public T? PopulateWithConverter<T>(JsonReader reader, T? value, bool requireConverter = true)
    {
        return (T?)PopulateWithConverter(reader, value, typeof(T), requireConverter);
    }

    public object? PopulateWithConverter(JsonReader reader, object? value, Type objectType, bool requireConverter = true)
    {
        JsonConverter? matchedConverter = null;

        foreach (var converter in Converters)
        {
            if (converter.CanConvert(objectType))
            {
                matchedConverter = converter;

                break;
            }
        }

        if (matchedConverter == null)
        {
            if (requireConverter)
            {
                throw new InvalidOperationException($"No matching converter was found for {objectType} and requireConverter is set to true (default)");
            }

            if (value == null)
            {
                throw new ArgumentException("Provided value was null and no matching converter was found");
            }

            Populate(reader, value);

            return value;
        }

        return matchedConverter.ReadJson(reader, objectType, value, this);
    }
}

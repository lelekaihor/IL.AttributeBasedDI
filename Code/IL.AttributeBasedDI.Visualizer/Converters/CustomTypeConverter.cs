using Newtonsoft.Json;

namespace IL.AttributeBasedDI.Visualizer.Converters;

public sealed class CustomTypeConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        // Only handle Type objects
        if (value is Type type)
        {
            writer.WriteValue(type.FullName);
        }
        else
        {
            writer.WriteNull();
        }

    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(Type).IsAssignableFrom(objectType);
    }
}
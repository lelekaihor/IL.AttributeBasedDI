using Newtonsoft.Json;

namespace IL.AttributeBasedDI.Visualizer.Converters;

public sealed class CustomTypeConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        // Only handle Type objects
        if (value is Type type)
        {
            writer.WriteValue(BeautifyType(type));

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
    
    public static string BeautifyType(Type type)
    {
        var assemblyName = type.Assembly.GetName().Name;
        var simpleName = GetTypeSignature(type, false);
        var fullName = GetTypeSignature(type, true);

        return $"{simpleName}|{fullName}, {assemblyName}";
    }

    private static string GetTypeSignature(Type type, bool useFullName)
    {
        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            var baseName = useFullName
                ? genericTypeDef.FullName?.Split('`')[0]
                : genericTypeDef.Name.Split('`')[0];

            var genericArgs = type.IsGenericTypeDefinition
                ? genericTypeDef.GetGenericArguments()
                : type.GetGenericArguments();

            var argNames = genericArgs
                .Select(arg =>
                    arg.IsGenericParameter
                        ? arg.Name
                        : GetTypeSignature(arg, useFullName)
                )
                .ToArray();

            return $"{baseName}<{string.Join(", ", argNames)}>";
        }

        return useFullName
            ? type.FullName ?? type.Name
            : type.Name;
    }

}
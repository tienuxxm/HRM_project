using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Abstractions.Converter;

public class DecimalAsStringJsonConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((long)value);
    }
}
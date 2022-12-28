using System.Text.Json;
using System.Text.Json.Serialization;

class CustomTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var input = reader.GetString();

        if (input is null)
            return TimeSpan.Zero;

        if (TimeSpan.TryParseExact(input, @"hh\:mm\:ss", null, out var asTimeSpan))
            return asTimeSpan;

        return TimeSpan.ParseExact(input, @"mm\:ss", null);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        Write(writer, value, options);
    }
}
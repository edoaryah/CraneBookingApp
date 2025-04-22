using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspnetCoreMvcFull.Converters
{
  /// <summary>
  /// Konverter untuk menampilkan DateTime hanya sampai level detik (tanpa milidetik)
  /// </summary>
  public class DateTimeSecondsPrecisionConverter : JsonConverter<DateTime>
  {
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      if (reader.TokenType == JsonTokenType.String)
      {
        if (DateTime.TryParse(reader.GetString(), out DateTime date))
        {
          return date;
        }
      }
      return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
      writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
  }

  /// <summary>
  /// Konverter untuk menampilkan tanggal saja tanpa komponen waktu
  /// </summary>
  public class DateOnlyJsonConverter : JsonConverter<DateTime>
  {
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      if (reader.TokenType == JsonTokenType.String)
      {
        if (DateTime.TryParse(reader.GetString(), out DateTime date))
        {
          return date.Date; // Gunakan kind asal dari parsing
        }
      }
      return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
      // Format tanggal saja: "yyyy-MM-dd"
      writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
  }
}

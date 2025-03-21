using System.Text.Json;
using System.Text.Json.Serialization;
using AspnetCoreMvcFull.Helpers;

namespace AspnetCoreMvcFull.Converters
{
  // Custom converter untuk JSON serialization/deserialization
  public class DateTimeConverter : JsonConverter<DateTime>
  {
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      // Saat membaca dari JSON (input dari pengguna), konversi dari WITA ke UTC
      var dateTime = reader.GetDateTime();

      // Jika tidak memiliki informasi timezone, anggap sebagai WITA (local)
      if (dateTime.Kind == DateTimeKind.Unspecified)
      {
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
      }

      return TimeZoneHelper.WitaToUtc(dateTime);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
      // Saat menulis ke JSON (output ke pengguna), konversi dari UTC ke WITA
      DateTime witaDateTime = TimeZoneHelper.UtcToWita(value);
      writer.WriteStringValue(witaDateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
  }
}

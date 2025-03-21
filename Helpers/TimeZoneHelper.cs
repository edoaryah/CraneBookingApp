namespace AspnetCoreMvcFull.Helpers
{
  public static class TimeZoneHelper
  {
    // WITA adalah UTC+8
    private static readonly TimeSpan WitaOffset = TimeSpan.FromHours(8);

    // Konversi dari UTC ke WITA
    public static DateTime UtcToWita(DateTime utcDateTime)
    {
      // Jika input bukan UTC, kemungkinan sudah dalam WITA atau dikonversi sebelumnya
      // Jadi jangan lakukan konversi lagi
      if (utcDateTime.Kind != DateTimeKind.Utc)
      {
        return utcDateTime;
      }

      DateTime witaDateTime = utcDateTime.Add(WitaOffset);
      return DateTime.SpecifyKind(witaDateTime, DateTimeKind.Local);
    }

    // Konversi dari WITA ke UTC
    public static DateTime WitaToUtc(DateTime witaDateTime)
    {
      // Jika sudah dalam UTC, jangan konversi lagi
      if (witaDateTime.Kind == DateTimeKind.Utc)
      {
        return witaDateTime;
      }

      // Anggap input adalah WITA (local) jika tidak ditentukan
      if (witaDateTime.Kind != DateTimeKind.Local && witaDateTime.Kind != DateTimeKind.Unspecified)
      {
        witaDateTime = DateTime.SpecifyKind(witaDateTime, DateTimeKind.Local);
      }

      // Konversi dari WITA ke UTC
      DateTime utcDateTime = witaDateTime.Subtract(WitaOffset);
      return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
    }

    // Helper untuk menentukan apakah DateTime perlu konversi
    public static bool IsUtc(DateTime dateTime)
    {
      return dateTime.Kind == DateTimeKind.Utc;
    }
  }
}

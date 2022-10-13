namespace RomgleWebApi.Utils
{
    public static class DateTimeUtils
    {
        public static string UtcNowDateString => DateTime.UtcNow.ToString("dd/MM/yyyy");
        
        public static string UtcNowTimeString => DateTime.UtcNow.ToString("HH:mm:ss");

    }
}

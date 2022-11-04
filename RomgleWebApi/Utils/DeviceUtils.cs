using RomgleWebApi.Data.Models.Auth;

namespace RomgleWebApi.Utils
{
    public static class DeviceUtils
    {
        public static Device Create(
            string deviceId,
            string personalKey,
            string personalKeyEncoding = "ASCII")
        {
            return new Device
            {
                Id = deviceId,
                PersonalKey = personalKey,
                PersonalKeyEncoding = personalKeyEncoding
            };
        }

        public static string GenerateDeviceId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}

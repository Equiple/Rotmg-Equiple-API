using RomgleWebApi.Utils;

namespace RomgleWebApi.ModelBinding.Attributes
{
    public class DeviceIdAttribute : FromUserAttribute
    {
        public DeviceIdAttribute() : base(CustomClaimNames.DeviceId)
        {
        }
    }
}

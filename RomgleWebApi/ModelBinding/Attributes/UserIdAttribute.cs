using RomgleWebApi.Utils;

namespace RomgleWebApi.ModelBinding.Attributes
{
    public class UserIdAttribute : FromUserAttribute
    {
        public UserIdAttribute() : base(CustomClaimNames.UserId)
        {
        }
    }
}

using RomgleWebApi.Utils;

namespace RomgleWebApi.ModelBinding.Attributes
{
    public class RefreshTokenAttribute : FromCookiesAttribute
    {
        public RefreshTokenAttribute() : base(CustomCookieKeys.RefreshToken)
        {
        }
    }
}

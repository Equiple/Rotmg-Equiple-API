using RotmgleWebApi.Authentication;

namespace RotmgleWebApi.ModelBinding
{
    public class UserIdAttribute : FromUserAttribute
    {
        public UserIdAttribute() : base(CustomClaimNames.UserId)
        {
        }
    }
}

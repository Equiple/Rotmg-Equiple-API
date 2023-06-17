namespace RotmgleWebApi.Authentication
{
    public class AccessTokenValidationException : Exception
    {
        public AccessTokenValidationException(string? message, bool tokenNotFound = false) : base(message)
        {
            TokenNotFound = tokenNotFound;
        }

        public bool TokenNotFound { get; }
    }
}

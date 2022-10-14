namespace RomgleWebApi.Utils
{
    public static class StaticRegistrationHelper
    {
        private static bool _scopeRegistered = false;

        public static void Scope(Action registration)
        {
            if (_scopeRegistered)
            {
                return;
            }
            registration.Invoke();
            _scopeRegistered = true;
        }
    }
}

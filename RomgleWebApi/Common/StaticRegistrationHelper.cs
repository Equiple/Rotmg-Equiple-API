namespace RotmgleWebApi
{
    public static class StaticRegistrationHelper
    {
        private static readonly Dictionary<Environment, HashSet<string>> _registeredIds;
        private static Environment _env = Environment.Prod;

        static StaticRegistrationHelper()
        {
            _registeredIds = Enum.GetValues<Environment>()
                .ToDictionary(env => env, _ => new HashSet<string>());
        }

        public static void SetTesting()
        {
            _env = Environment.Test;
        }

        public static void ProdOnce(string id, Action registration)
        {
            EnvOnce(Environment.Prod, id, registration);
        }

        public static void TestOnce(string id, Action registration)
        {
            EnvOnce(Environment.Test, id, registration);
        }

        public static void Once(string id, Action registration)
        {
            Once(Environment.Any, id, registration);
        }

        private static void EnvOnce(Environment env, string id, Action registration)
        {
            if (_env == env)
            {
                Once(env, id, registration);
            }
        }

        private static void Once(Environment env, string id, Action registration)
        {
            if (_registeredIds[env].Add(id))
            {
                registration.Invoke();
            }
        }

        private enum Environment
        {
            Any,
            Prod,
            Test
        }
    }
}

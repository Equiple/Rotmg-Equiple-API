namespace RotmgleWebApi
{
    public abstract record class Result<T, E>(bool IsOk) where E : Exception
    {
        public bool IsError => !IsOk;

        public record class Ok(T Value) : Result<T, E>(true);

        public record class Error(E Exception) : Result<T, E>(false);

        public static implicit operator Result<T, E>(T value)
        {
            return new Ok(value);
        }

        public static implicit operator Result<T, E>(E exception)
        {
            return new Error(exception);
        }
    }

    public abstract record class Result<T>(bool IsOk) : Result<T, Exception>(IsOk)
    {
        public new record class Ok(T Value) : Result<T>(true);

        public new record class Error(Exception Exception) : Result<T>(false);

        public static implicit operator Result<T>(T value)
        {
            return new Ok(value);
        }

        public static implicit operator Result<T>(Exception exception)
        {
            return new Error(exception);
        }
    }
}

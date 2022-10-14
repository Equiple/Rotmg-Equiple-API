namespace RomgleWebApi.Extensions
{
    public static class EnumerableExtensions
    {
        public static int FirstIndex<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            int index = 0;
            foreach (T item in collection)
            {
                if (predicate.Invoke(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }
}

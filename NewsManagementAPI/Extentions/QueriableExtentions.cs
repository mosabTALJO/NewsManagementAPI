namespace NewsManagementAPI.Extentions
{
    public static class QueriableExtentions
    {
        public static IQueryable<T> SkipTake<T>(this IQueryable<T> source, int pageNo, int pageSize)
            => source.Skip((pageNo - 1) * pageSize).Take(pageSize);
    }
}

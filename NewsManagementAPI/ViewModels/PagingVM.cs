namespace NewsManagementAPI.ViewModels
{
    public class PagingVM<T> where T : class
    {
        public int Count { get; set; }
        public T[] Items { get; set; } = [];
    }
}

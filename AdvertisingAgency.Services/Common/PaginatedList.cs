namespace AdvertisingAgency.Services.Common
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public int PageIndex { get; set; }

        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public PaginatedList(List<T> items, int totalItems, int page, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            Page = page;
            PageSize = pageSize;
        }
    }
}

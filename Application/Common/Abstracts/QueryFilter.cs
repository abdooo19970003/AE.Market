namespace AE.Market.Application.Common.Abstracts
{
    public record QueryFilter
    {
        public QueryFilter(
            string? sortBy,
            bool? sortDescending,
            string? search,
            bool isActive = true
        )
        {
            SortBy = sortBy;
            SortDescending = sortDescending;
            Search = search;
            IsActive = isActive;
        }

        public QueryFilter(
            int pageNumber,
            int pageSize,
            string? sortBy,
            bool? sortDescending,
            string? search,
            bool isActive = true
        )
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SortBy = sortBy;
            SortDescending = sortDescending;
            Search = search;
            IsActive = isActive;
        }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool? SortDescending { get; set; } = false;
        public string? Search { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

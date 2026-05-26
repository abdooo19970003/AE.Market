namespace AE.Market.Application.Common.DTOs
{
    internal class QueryFilter
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public SortOrder SortOrder { get; set; } = SortOrder.DESC;
        public string? Search { get; set; }
    }
    public enum SortOrder { ASC, DESC };
}

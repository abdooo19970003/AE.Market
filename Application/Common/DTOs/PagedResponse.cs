using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Common.DTOs
{
    public class PagedResponse<T>(IReadOnlyList<T>? data, int pageNumber, int pageSize, int totalRecords)
    {
        public IReadOnlyList<T>? Data { get; init; } = data;
        public int PageNumber { get; init; } = pageNumber;
        public int PageSize { get; init; } = pageSize;
        public int TotalRecords { get; init; } = totalRecords;
        public int TotalPages => (int) Math.Ceiling((decimal)(TotalRecords / PageSize)); 
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
     
    }
}

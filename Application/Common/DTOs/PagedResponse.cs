using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Common.DTOs
{
    public class PagedResponse<T> 
    {
        public PagedResponse(IReadOnlyList<T>? data, int pageNumber, int pageSize, int totalRecords)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
        }

        public IReadOnlyList<T>? Data { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalRecords { get; init; }
        public int TotalPages => (int) Math.Ceiling((decimal)(TotalRecords / PageSize)); 
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
     
    }
}

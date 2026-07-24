namespace Mebabl.Platform.Application.Common.Pagination;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
using Domain.Enums;

namespace Application.DTOs
{
    public class ReportFilterRequest
    {
        public ReportStatus? Status { get; init; }
        public Priority? Priority { get; init; }
        public string? Location { get; init; }
        public string OrderBy { get; init; } = "createdAt";
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}

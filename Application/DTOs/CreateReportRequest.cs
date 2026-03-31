using Domain.Enums;

namespace Application.DTOs
{
    public class CreateReportRequest
    {
        public string Title { get; init; } = default!;
        public string Description { get; init; } = default!;
        public string Location { get; init; } = default!;
        public Priority Priority { get; init; }
    }
}

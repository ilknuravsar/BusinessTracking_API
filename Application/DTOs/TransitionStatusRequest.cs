using Domain.Enums;

namespace Application.DTOs
{
    public class TransitionStatusRequest
    {
        public ReportStatus NewStatus { get; init; }
    }
}

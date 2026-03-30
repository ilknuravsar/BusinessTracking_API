using Domain.Enums;

namespace Domain.Exceptions
{
    public class InvalidStateTransitionException : DomainException
    {
        public ReportStatus From { get; }
        public ReportStatus To { get; }

        public InvalidStateTransitionException(ReportStatus from, ReportStatus to)
            : base($"{from} durumundan {to} durumuna geçiş yapılamaz.")
        {
            From = from;
            To = to;
        }
    }
}

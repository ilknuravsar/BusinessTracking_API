using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities
{
    public class OutageReport
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Location { get; set; } = default!;
        public Priority Priority { get; set; }
        public ReportStatus Status { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public AppUser? CreatedBy { get; private set; }

        private static readonly Dictionary<ReportStatus, ReportStatus[]> _allowedTransitions = new()
        {
            [ReportStatus.New] = [ReportStatus.UnderReview, ReportStatus.Cancelled],
            [ReportStatus.UnderReview] = [ReportStatus.Assigned, ReportStatus.Unfounded, ReportStatus.Cancelled],
            [ReportStatus.Assigned] = [ReportStatus.InProgress, ReportStatus.Cancelled],
            [ReportStatus.InProgress] = [ReportStatus.Completed, ReportStatus.Cancelled],
            [ReportStatus.Completed] = [],  // Tamamlandı
            [ReportStatus.Cancelled] = [],  //  İptal
            [ReportStatus.Unfounded] = [],  // Asılsız
        };

        public OutageReport() { }

   
        public void TransitionTo(ReportStatus newStatus, bool isAdmin)
        {
           
            if (IsTerminal())
                throw new InvalidStateTransitionException(Status, newStatus);

            
            if (!isAdmin && newStatus != ReportStatus.Cancelled)
                throw new ForbiddenException("Bu durum geçişini sadece Admin yapabilir.");

            var allowed = _allowedTransitions[Status];

            if (!allowed.Contains(newStatus))
                throw new InvalidStateTransitionException(Status, newStatus);

            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsTerminal() =>
            Status is ReportStatus.Completed
                    or ReportStatus.Cancelled
                    or ReportStatus.Unfounded;

        public bool CanBeViewedBy(Guid userId, bool isAdmin) =>
            isAdmin || CreatedById == userId;
    }
}

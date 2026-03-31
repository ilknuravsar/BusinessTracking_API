using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IOutageReportRepository
    {
        Task<OutageReport?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PaginatedResult<OutageReport>> GetPagedAsync(
            ReportStatus? status,
            Priority? priority,
            string? location,
            string orderBy,   
            int page,
            int pageSize,
            Guid? userId,    
            CancellationToken ct = default);

        Task<bool> ExistsRecentByLocationAsync(
            string location,
            CancellationToken ct = default);

        Task AddAsync(OutageReport report, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        Task UpdateAsync(OutageReport report);


    }
}

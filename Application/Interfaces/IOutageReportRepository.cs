using Application.Common;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOutageReportRepository
    {
        Task<OutageReport?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PaginatedResult<OutageReport>> GetPagedAsync(
            ReportStatus? status,
            Priority? priority,
            string? location,
            string orderBy,   // priority-createdAt
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

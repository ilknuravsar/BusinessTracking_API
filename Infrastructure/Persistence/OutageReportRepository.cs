using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class OutageReportRepository : IOutageReportRepository
    {
        private readonly AppDbContext _db;

        public OutageReportRepository(AppDbContext db) => _db = db;


        public async Task<OutageReport?> GetByIdAsync(Guid id, CancellationToken ct)
            => await _db.OutageReports
                   .Include(r => r.CreatedBy)  
                   .FirstOrDefaultAsync(r => r.Id == id, ct);

    
        public async Task<PaginatedResult<OutageReport>> GetPagedAsync(ReportStatus? status,Priority? priority,string? location,string orderBy,int page,int pageSize,Guid? userId,CancellationToken ct)
        {
            var query = _db.OutageReports
                           .Include(r => r.CreatedBy)
                           .AsQueryable();

           
            if (userId.HasValue)
                query = query.Where(r => r.CreatedById == userId.Value);

           
            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(r => r.Priority == priority.Value);

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(r => r.Location.Contains(location));

         
            query = orderBy.ToLower() == "priority"
                ? query.OrderByDescending(r => r.Priority)
                : query.OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PaginatedResult<OutageReport>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

      
        public async Task<bool> ExistsRecentByLocationAsync(
            string location, CancellationToken ct)
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            return await _db.OutageReports.AnyAsync(r =>
                r.Location == location &&
                r.CreatedAt >= oneHourAgo, ct);
        }

        
        public async Task AddAsync(OutageReport report, CancellationToken ct)
            => await _db.OutageReports.AddAsync(report, ct);

        public async Task SaveChangesAsync(CancellationToken ct)
            => await _db.SaveChangesAsync(ct);

        public async Task UpdateAsync(OutageReport report)
        {
            _db.OutageReports.Update(report);
            await _db.SaveChangesAsync();
        }
    }
}

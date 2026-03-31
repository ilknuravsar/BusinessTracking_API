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
    public interface IOutageReportService
    {
        Task<ApiResponse<OutageReportDto>> CreateAsync(CreateReportRequest request,Guid createdById,CancellationToken ct = default);

        OutageReport Create(string title, string description, string location, Priority priority, Guid createdById, ReportStatus status);

        Task<ApiResponse<PaginatedResult<OutageReportDto>>> GetAllAsync(ReportFilterRequest filter,Guid? userId,CancellationToken ct = default);

        Task<ApiResponse<OutageReportDto>> GetByIdAsync(Guid reportId,Guid userId,bool isAdmin,CancellationToken ct = default);

        Task<ApiResponse<OutageReportDto>> TransitionStatusAsync(Guid reportId,ReportStatus newStatus, bool isAdmin, CancellationToken ct = default);

        Task<ApiResponse<OutageReportDto>> UpdateAsync(Guid id, UpdateOutageRequest request, Guid currentUserId, bool isAdmin);


    }
}

using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Logging; 

namespace Application.Services
{
    public class OutageReportService : IOutageReportService
    {
        private readonly IOutageReportRepository _repo;
        private readonly ILogger<OutageReportService> _logger;
        private readonly IMapper _mapper;

        public OutageReportService(IOutageReportRepository repo, ILogger<OutageReportService> logger,IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResponse<OutageReportDto>> CreateAsync(CreateReportRequest request, Guid createdById, CancellationToken ct = default)
        {
          
            var exists = await _repo.ExistsRecentByLocationAsync(request.Location, ct);
            if (exists)
            {
            
                _logger.LogWarning("Yeni bildirim reddedildi. Lokasyon: {Location} için son 1 saat içinde zaten kayıt mevcut. Oluşturan: {UserId}", request.Location, createdById);
                throw new DuplicateLocationException(request.Location);
            }

            //var report = OutageReport.Create(
            //    request.Title,
            //    request.Description,
            //    request.Location,
            //    request.Priority,
            //    createdById);

            var report = Create(
                    request.Title,
                    request.Description,
                    request.Location,
                    request.Priority,
                    createdById
                );

            await _repo.AddAsync(report, ct);
            await _repo.SaveChangesAsync(ct);

           
            _logger.LogInformation("Yeni arıza bildirimi oluşturuldu. ID: {ReportId}, Başlık: {Title}", report.Id, report.Title);
            var dto = _mapper.Map<OutageReportDto>(report);
            return ApiResponse<OutageReportDto>.Ok(
                dto,
                //ToDto(report),
                "Report created successfully.");
        }

      public OutageReport Create(string title,string description,string location,Priority priority, Guid createdById)
      {
            return new OutageReport
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                Location = location,
                Priority = priority,
                Status = ReportStatus.New,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }


        public async Task<ApiResponse<PaginatedResult<OutageReportDto>>> GetAllAsync(ReportFilterRequest filter, Guid? userId, CancellationToken ct = default)
        {
            var paged = await _repo.GetPagedAsync(
                filter.Status,
                filter.Priority,
                filter.Location,
                filter.OrderBy,
                filter.Page,
                filter.PageSize,
                userId,
                ct);

            var result = new PaginatedResult<OutageReportDto>
            {
                Items = paged.Items.Select(ToDto).ToList(),
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };

            return ApiResponse<PaginatedResult<OutageReportDto>>.Ok(result);
        }

      
        public async Task<ApiResponse<OutageReportDto>> GetByIdAsync(Guid reportId, Guid userId, bool isAdmin, CancellationToken ct = default)
        {
            var report = await _repo.GetByIdAsync(reportId, ct);

            if (report == null)
            {
                _logger.LogWarning("Rapor bulunamadı. ID: {ReportId}", reportId);
                throw new NotFoundException($"Report {reportId} not found.");
            }

            if (!report.CanBeViewedBy(userId, isAdmin))
            {
                _logger.LogWarning("Yetkisiz erişim denemesi. User: {UserId}, Rapor ID: {ReportId}", userId, reportId);
                throw new ForbiddenException("You do not have access to this report.");
            }
            var dto = _mapper.Map<OutageReportDto>(report);
            return ApiResponse<OutageReportDto>.Ok(dto);
        }

      
        public async Task<ApiResponse<OutageReportDto>> TransitionStatusAsync(Guid reportId, ReportStatus newStatus, bool isAdmin, CancellationToken ct = default)
        {
            var report = await _repo.GetByIdAsync(reportId, ct);

            if (report == null)
            {
                _logger.LogWarning("Durum güncellemesi başarısız: Rapor bulunamadı. ID: {ReportId}", reportId);
                throw new NotFoundException($"Report {reportId} not found.");
            }

            var oldStatus = report.Status;

          
            report.TransitionTo(newStatus, isAdmin);

            await _repo.SaveChangesAsync(ct);

         
            _logger.LogInformation("Rapor durumu güncellendi. ID: {ReportId}, Geçiş: {OldStatus} -> {NewStatus}, İşlemi Yapan: Admin",
                reportId, oldStatus, newStatus);
            var dto = _mapper.Map<OutageReportDto>(report);

            return ApiResponse<OutageReportDto>.Ok(
                dto,
                //ToDto(report),
                "Status updated successfully.");
        }

     
        private static OutageReportDto ToDto(OutageReport r) => new()
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            Location = r.Location,
            Priority = r.Priority.ToString(),
            Status = r.Status.ToString(),
            CreatedByName = r.CreatedBy?.UserName ?? "",
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };

        public async Task<ApiResponse<OutageReportDto>> UpdateAsync(Guid id, UpdateOutageRequest request, Guid currentUserId, bool isAdmin)
        {
        
            var report = await _repo.GetByIdAsync(id);

    
            if (report == null)
            {
                return new ApiResponse<OutageReportDto>
                {
                    Success = false,
                    Message = "Güncellenmek istenen arıza kaydı bulunamadı."
                };
            }

            
            if (!isAdmin && report.CreatedById != currentUserId)
            {
                return new ApiResponse<OutageReportDto>
                {
                    Success = false,
                    Message = "Bu işlem için yetkiniz bulunmamaktadır (Sadece kendi kayıtlarınız)."
                };
            }

            report.Title = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : report.Title;
            report.Description = !string.IsNullOrWhiteSpace(request.Description) ? request.Description : report.Description;
            report.Location = !string.IsNullOrWhiteSpace(request.Location) ? request.Location : report.Location;
            if (request.Priority != 0 )
            {

                if (Enum.IsDefined(typeof(Priority), request.Priority))
                {
                    report.Priority = request.Priority;
                }
                else
                {
                    return new ApiResponse<OutageReportDto>
                    {
                        Success = false,
                        Message = "Geçersiz öncelik değeri! (Sadece 1-Düşük, 2-Orta, 3-Yüksek kabul edilir)"
                    };
                }
            } 
                report.UpdatedAt = DateTime.UtcNow;

          
            await _repo.UpdateAsync(report);

           
          

            var dto = _mapper.Map<OutageReportDto>(report);

            return new ApiResponse<OutageReportDto>(dto);
        }


    }
}
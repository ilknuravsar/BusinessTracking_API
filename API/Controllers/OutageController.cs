using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class OutageController : ControllerBase
    {
        private readonly IOutageReportService _service;

        public OutageController(IOutageReportService service)
        {
            _service = service;
        }

        /// <summary>
        /// Sisteme yeni bir arıza bildirimi kaydeder.
        /// </summary>
        /// <remarks>
        /// <b>İş Kuralı:</b> Aynı lokasyona 1 saat içinde ikinci bildirim yapılamaz.
        /// </remarks>
        /// <param name="request">Bildirim başlığı, açıklaması, lokasyonu ve önceliği.</param>
        /// <response code="201">Bildirim başarıyla oluşturuldu.</response>
        /// <response code="400">Lokasyon kuralı ihlali veya geçersiz veri.</response>
        /// <response code="401">Yetkisiz erişim (Token eksik veya geçersiz).</response>
        [HttpPost("Create")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ApiResponse<OutageReportDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<OutageReportDto>>> Create([FromBody] CreateReportRequest request)
        {
            var userId = GetUserId();
            var result = await _service.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }


        /// <summary>
        /// Arıza bildirimlerini filtreleyerek ve sayfalayarak listeler.
        /// </summary>
        /// <remarks>
        /// <b>Yetki:</b> Admin tüm kayıtları, User ise sadece kendi oluşturduğu kayıtları görür.
        /// </remarks>
        /// <param name="filter">Durum, öncelik, lokasyon ve sayfalama parametreleri.</param>
        /// <response code="200">Listeleme başarılı.</response>
        [HttpGet("GetAll")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<OutageReportDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedResult<OutageReportDto>>>> GetAll([FromQuery] ReportFilterRequest filter)
        {
            var userId = User.IsInRole("Admin") ? null : (Guid?)GetUserId();
            var result = await _service.GetAllAsync(filter, userId);
            return Ok(result);
        }

        /// <summary>
        /// Belirli bir arıza bildiriminin detaylarını getirir.
        /// </summary>
        /// <remarks>
        /// <b>Güvenlik:</b> Kullanıcı sadece kendi kaydına erişebilir (Admin hariç).
        /// </remarks>
        /// <param name="id">Raporun benzersiz Guid değeri.</param>
        /// <response code="200">Rapor bulundu ve getirildi.</response>
        /// <response code="403">Bu raporu görmeye yetkiniz yok.</response>
        /// <response code="404">Rapor sistemde bulunamadı.</response>
        [HttpGet("GetById/{id}")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ApiResponse<OutageReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<OutageReportDto>>> GetById(Guid id)
        {
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            var result = await _service.GetByIdAsync(id, userId, isAdmin);
            return Ok(result);
        }

    
        /// <summary>
        /// Bir arıza bildiriminin durumunu günceller.
        /// </summary>
        /// <remarks>
        /// <b>Yetki:</b> Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.<br/>
        /// <b>Durum Makinesi:</b> Sadece izin verilen durum geçişleri yapılabilir (Örn: New -> UnderReview).
        /// </remarks>
        /// <param name="id">Rapor ID'si.</param>
        /// <param name="request">Yeni durum değeri (ReportStatus).</param>
        /// <response code="200">Durum başarıyla güncellendi.</response>
        /// <response code="422">Geçersiz durum geçişi (Durum makinesi kuralı).</response>
        /// <response code="403">Bu işlem için Admin yetkisi gerekiyor.</response>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ApiResponse<OutageReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ApiResponse<OutageReportDto>>> TransitionStatus(Guid id, [FromBody] TransitionStatusRequest request)
        {
            var result = await _service.TransitionStatusAsync(id, request.NewStatus, true);
            return Ok(result);
        }


        /// <summary>
        /// Mevcut bir arıza kaydını günceller.
        /// </summary>
        /// <remarks>
        /// <b>Yetki Kuralları:</b>
        /// - Admin: Tüm kayıtları güncelleyebilir.
        /// - User: Sadece kendi oluşturduğu kayıtları güncelleyebilir.
        /// - Durum (Status) bu metotla değiştirilemez.
        /// </remarks>
        /// <param name="id">Güncellenecek kaydın benzersiz kimliği (Guid)</param>
        /// <param name="request">Güncellenecek alanlar (Başlık, Açıklama, Konum, Öncelik)</param>
        /// <returns>Güncellenmiş arıza kaydı verisi</returns>
        [HttpPut("{id}")]
        [EnableRateLimiting("fixed")]
        [Authorize] 
        public async Task<ActionResult<ApiResponse<OutageReportDto>>> Update(Guid id, [FromBody] UpdateOutageRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Oturum geçersiz." });

            var currentUserId = Guid.Parse(userIdClaim);

          
            var isAdmin = User.IsInRole("Admin");
            var result = await _service.UpdateAsync(id, request, currentUserId, isAdmin);

            if (!result.Success)
            {

                return BadRequest(result);
            }

            return Ok(result);
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim == null ? Guid.Empty : Guid.Parse(claim.Value);
        }
    }
}
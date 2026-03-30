using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.Services
{
    public class OutageReportServiceTests
    {
        private readonly Mock<IOutageReportRepository> _repoMock;
        private readonly Mock<ILogger<OutageReportService>> _loggerMock;
        private readonly OutageReportService _service;
        private readonly IMapper _mapper;
        public OutageReportServiceTests()
        {
            _repoMock = new Mock<IOutageReportRepository>(); 
            _loggerMock = new Mock<ILogger<OutageReportService>>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>(); 
            });
            _mapper = configuration.CreateMapper();

            _service = new OutageReportService(_repoMock.Object, _loggerMock.Object,_mapper);

         
        }

       

        [Fact]
        public async Task Create_DuplicateLocation_ThrowsException()
        {
         
            var request = new CreateReportRequest
            {
                Title = "Test Arıza",
                Location = "Isparta/Merkez",
                Priority = Priority.High
            };
            var userId = Guid.NewGuid();

            _repoMock.Setup(x => x.ExistsRecentByLocationAsync(request.Location, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

         
            await Assert.ThrowsAsync<DuplicateLocationException>(() =>
                _service.CreateAsync(request, userId));
        }

        

        [Fact]
        public async Task Update_Forbidden_ForNonOwne()
        {
         
            var reportId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();

            var existingReport = new OutageReport
            {
                Id = reportId,
                CreatedById = ownerId,
                Title = "Orijinal Başlık"
            };

            _repoMock.Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>())).ReturnsAsync(existingReport);

            var updateRequest = new UpdateOutageRequest { Title = "Yetkisiz Değişiklik" };

            var result = await _service.UpdateAsync(reportId, updateRequest, strangerId, isAdmin: false);

            Assert.False(result.Success);

            Assert.Equal("Bu işlem için yetkiniz bulunmamaktadır (Sadece kendi kayıtlarınız).", result.Message);
        }

        [Fact]
        public async Task Update_Owner_Succeeds()
        {
            var reportId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingReport = new OutageReport { Id = reportId, CreatedById = userId, Title = "Eski" };
            _repoMock.Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>())).ReturnsAsync(existingReport);

            var updateRequest = new UpdateOutageRequest { Title = "Yeni Başlık" };

            
            var result = await _service.UpdateAsync(reportId, updateRequest, userId, isAdmin: false);

         
            Assert.True(result.Success);
            Assert.Equal("Yeni Başlık", result.Data.Title);
            _repoMock.Verify(x => x.UpdateAsync(It.IsAny<OutageReport>()), Times.Once);
        }


        [Fact]
        public async Task Update_InvalidPriority_Fails()
        {
    
            var reportId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingReport = new OutageReport { Id = reportId, CreatedById = userId };

            _repoMock.Setup(x => x.GetByIdAsync(reportId, It.IsAny<CancellationToken>())).ReturnsAsync(existingReport);

           
            var updateRequest = new UpdateOutageRequest { Priority = (Priority)99 };

          
            var result = await _service.UpdateAsync(reportId, updateRequest, userId, isAdmin: false);

          
            Assert.False(result.Success);
            Assert.Contains("Geçersiz öncelik", result.Message);
        }

    }
}
using Application.DTOs;
using AutoMapper;
using Domain.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<OutageReport, OutageReportDto>()
            //.ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            //.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : "")).ReverseMap();
    }
}
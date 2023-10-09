using AssetManagement.Dtos;
using AssetManagement.Models;
using AutoMapper;

namespace AssetManagement;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Position, PositionDto>();
    }
}
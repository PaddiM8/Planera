using AutoMapper;

namespace Planera.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, Author>()
            .ForMember(
                dest => dest.UserName,
                opt => opt.MapFrom(src => src.UserName)
            )
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Email)
            );

        CreateMap<Project, Project>()
            .ForMember(
                dest => dest.Author,
                opt => opt.MapFrom(src => src.InternalAuthor)
            )
            .ForMember(
                dest => dest.InternalAuthor,
                opt => opt.MapFrom<User>(_ => null!)
            );
    }
}
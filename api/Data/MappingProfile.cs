using AutoMapper;
using Planera.Data.Dto;

namespace Planera.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, AccountDto>()
            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => src.UserName)
            );
        CreateMap<Project, ProjectDto>();
        CreateMap<Ticket, TicketDto>()
            .ForMember(
                dest => dest.ProjectSlug,
                opt => opt.MapFrom(src => src.Project.Slug)
            );
        CreateMap<Invitation, InvitationDto>();
        CreateMap<Note, NoteDto>();
    }
}
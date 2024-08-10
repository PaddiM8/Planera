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
        CreateMap<Project, ProjectDto>()
            .ForMember(
                dest => dest.AllTicketsCount,
                opt => opt.MapFrom(src => src.Tickets.Count)
            )
            .ForMember(
                dest => dest.OpenTicketsCount,
                opt => opt.MapFrom(src =>
                    src.Tickets.Count(ticket => ticket.Status == TicketStatus.None)
                )
            )
            .ForMember(
                dest => dest.ClosedTicketsCount,
                opt => opt.MapFrom(src =>
                    src.Tickets.Count(ticket => ticket.Status == TicketStatus.Closed)
                )
            )
            .ForMember(
                dest => dest.InactiveTicketsCount,
                opt => opt.MapFrom(src =>
                    src.Tickets.Count(ticket => ticket.Status == TicketStatus.Inactive)
                )
            )
            .ForMember(
                dest => dest.DoneTicketsCount,
                opt => opt.MapFrom(src =>
                    src.Tickets.Count(ticket => ticket.Status == TicketStatus.Done)
                )
            );
        CreateMap<Ticket, TicketDto>()
            .ForMember(
                dest => dest.ProjectSlug,
                opt => opt.MapFrom(src => src.Project.Slug)
            )
            .ForMember(
                dest => dest.NoteCount,
                opt => opt.MapFrom(src => src.Notes.Count)
            );
        CreateMap<Invitation, InvitationDto>();
        CreateMap<Note, NoteDto>();
    }
}
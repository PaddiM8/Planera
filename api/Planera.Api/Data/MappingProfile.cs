using AutoMapper;
using Planera.Api.Data.Dto;
using Planera.Api.Data.Notifications;
using Planera.Api.Data.Projects;
using Planera.Api.Data.Tickets;
using Planera.Api.Data.Users;

namespace Planera.Api.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, AccountDto>()
            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => src.UserName)
            )
            .ForMember(
                dest => dest.HasPassword,
                opt => opt.MapFrom(src => src.PasswordHash != null)
            )
            .ForMember(
                dest => dest.EnabledNotificationKinds,
                opt => opt.MapFrom(src =>
                    Enum
                        .GetValues<NotificationKinds>()
                        .Where(x => src.EnabledNotificationKinds.HasFlag(x))
                        .ToList()
                )
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
                dest => dest.OpenTicketsWithDeadlineCount,
                opt => opt.MapFrom(src =>
                    src.Tickets.Count(ticket => ticket.Status == TicketStatus.None && ticket.Deadline != null)
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
        CreateMap<ProjectParticipant, ProjectParticipantDto>()
            .ForMember(
                dest => dest.EnabledNotificationKinds,
                opt => opt.MapFrom(src =>
                    Enum
                        .GetValues<NotificationKinds>()
                        .Where(x => src.EnabledNotificationKinds.HasFlag(x))
                        .ToList()
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
        CreateMap<NotificationTrigger, NotificationTriggerDto>();
    }
}
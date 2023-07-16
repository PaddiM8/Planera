using AutoMapper;
using Planera.Data.Dto;

namespace Planera.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.Id)
            )
            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => src.UserName)
            )
            .ForMember(
                dest => dest.AvatarPath,
                opt => opt.MapFrom(src => src.AvatarPath)
            );

        CreateMap<User, AccountDto>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.Id)
            )
            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => src.UserName)
            )
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Email)
            )
            .ForMember(
                dest => dest.AvatarPath,
                opt => opt.MapFrom(src => src.AvatarPath)
            );

        CreateMap<Project, ProjectDto>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.Id)
            )
            .ForMember(
                dest => dest.Author,
                opt => opt.MapFrom(src => src.Author)
            )
            .ForMember(
                dest => dest.Slug,
                opt => opt.MapFrom(src => src.Slug)
            )
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.Name)
            );

        CreateMap<Ticket, TicketDto>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.Id)
            )
            .ForMember(
                dest => dest.ProjectId,
                opt => opt.MapFrom(src => src.ProjectId)
            )
            .ForMember(
                dest => dest.ProjectSlug,
                opt => opt.MapFrom(src => src.Project.Slug)
            )
            .ForMember(
                dest => dest.Title,
                opt => opt.MapFrom(src => src.Title)
            )
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description)
            )
            .ForMember(
                dest => dest.Priority,
                opt => opt.MapFrom(src => src.Priority)
            )
            .ForMember(
                dest => dest.Assignees,
                opt => opt.MapFrom(src => src.Assignees)
            )
            .ForMember(
                dest => dest.Author,
                opt => opt.MapFrom(src => src.Author)
            );

        CreateMap<Invitation, InvitationDto>()
            .ForMember(
                dest => dest.Project,
                opt => opt.MapFrom(src => src.Project)
            )
            .ForMember(
                dest => dest.User,
                opt => opt.MapFrom(src => src.User)
            );
    }
}
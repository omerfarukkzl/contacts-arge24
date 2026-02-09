using AutoMapper;
using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Contacts;
using Contacts.Api.Dtos.Tags;

namespace Contacts.Api.Infrastructure;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Tag, TagDto>();

        CreateMap<Contact, ContactDto>()
            .ForMember(
                destination => destination.Tags,
                configuration => configuration.MapFrom(source => source.ContactTags.Select(contactTag => contactTag.Tag)));
    }
}

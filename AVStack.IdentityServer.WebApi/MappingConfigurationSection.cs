using AutoMapper;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Application;
using AVStack.IdentityServer.WebApi.Models.Application.Interfaces;
using AVStack.IdentityServer.WebApi.Models.Requests;

namespace AVStack.IdentityServer.WebApi
{
    public class MappingConfigurationSection : Profile
    {
        public MappingConfigurationSection()
        {
            CreateMap<IUser, UserEntity>().ReverseMap();
            CreateMap<User, UserEntity>().ReverseMap();

            CreateMap<SignUpRequest, User>()
                .ForMember(
                    destinationMember => destinationMember.UserName,
                    memberOptions =>
                        memberOptions.MapFrom(model => EmptyUserNameResolver(model)));
            CreateMap<SignUpRequest, UserEntity>()
                .ForMember(
                    destinationMember => destinationMember.UserName,
                    memberOptions =>
                        memberOptions.MapFrom(model => EmptyUserNameResolver(model)));
        }

        private string EmptyUserNameResolver(SignUpRequest user)
            => !string.IsNullOrEmpty(user.UserName) ? user.UserName : user.Email.Split('@')[0];
    }
}
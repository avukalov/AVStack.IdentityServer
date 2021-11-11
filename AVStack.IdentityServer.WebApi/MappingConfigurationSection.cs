using AutoMapper;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Application;
using AVStack.IdentityServer.WebApi.Models.Application.Interfaces;

namespace AVStack.IdentityServer.WebApi
{
    public class MappingConfigurationSection : Profile
    {
        public MappingConfigurationSection()
        {
            CreateMap<IUser, UserEntity>().ReverseMap();
            CreateMap<User, UserEntity>().ReverseMap();
        }
    }
}
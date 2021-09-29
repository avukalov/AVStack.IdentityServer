using AutoMapper;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Business;
using AVStack.IdentityServer.WebApi.Models.Business.Interfaces;

namespace AVStack.IdentityServer.WebApi.Common.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<IUser, UserEntity>().ReverseMap();
            CreateMap<User, UserEntity>().ReverseMap();
        }
    }
}
using AutoMapper;
using AVStack.IdentityServer.WebApi.Controllers;
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

            CreateMap<SignUpModel, UserEntity>()
                .ForMember(
                dest => dest.UserName,
                m =>
                    m.MapFrom(u => EmptyUserNameResolver(u)));
        }

        private string EmptyUserNameResolver(SignUpModel user)
        {
            return !string.IsNullOrEmpty(user.UserName) ? user.UserName : user.Email.Split('@')[0];
        }
    }
}
using System.Collections.Generic;
using AVStack.IdentityServer.WebApi.Models.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AVStack.IdentityServer.WebApi.Models.System
{
    public class IdentityResultModel
    {
        public bool Succeeded { get; set; }

        public List<IdentityError> Errors { get; set; }

        public IUser User { get; set; }
    }
}
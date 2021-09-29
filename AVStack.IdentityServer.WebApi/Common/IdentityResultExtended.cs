using System.Collections;
using System.Collections.Generic;
using AVStack.IdentityServer.WebApi.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace AVStack.IdentityServer.WebApi.Common
{
    public class IdentityResultExtended
    {
        public bool Succeeded { get; init; }
        public IList<IdentityError> Errors { get; init; }
        public UserEntity UserEntity { get; init; }
    }
}
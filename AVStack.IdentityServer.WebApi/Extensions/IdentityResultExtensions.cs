using System.Linq;
using AVStack.IdentityServer.WebApi.Models.System;
using Microsoft.AspNetCore.Identity;

namespace AVStack.IdentityServer.WebApi.Extensions
{
    public static class IdentityResultExtensions
    {
        public static IdentityResultModel ToIdentityResultModel(this IdentityResult result)
        {
            return new IdentityResultModel
            {
                Succeeded = result.Succeeded,
                Errors = result.Errors.ToList()
            };
        }

        public static IdentityResultModel ToIdentityResultModel(this SignInResult result)
        {
            var newResult = new IdentityResultModel
            {
                Succeeded = result.Succeeded
            };

            if (result.IsLockedOut)
            {
                newResult.Errors.Add(IdentityErrorExtensions.LockedOut);
            }
            else if (result.IsNotAllowed)
            {
                newResult.Errors.Add(IdentityErrorExtensions.NotAllowed);
            }
            else if (result.RequiresTwoFactor)
            {
                newResult.Errors.Add(IdentityErrorExtensions.RequiresTwoFactor);
            }

            return newResult;
        }
    }
}
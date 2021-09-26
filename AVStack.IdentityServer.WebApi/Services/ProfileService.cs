using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AVStack.IdentityServer.Constants;
using AVStack.IdentityServer.WebApi.Data.Entities;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AVStack.IdentityServer.WebApi.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<RoleEntity> _roleManager;

        public ProfileService(
            UserManager<UserEntity> userManager,
            RoleManager<RoleEntity> roleEntity)
        {
            _userManager = userManager;
            _roleManager = roleEntity;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = _userManager.GetUserAsync(context.Subject).Result;
            
            var claims = InitializeDefaultClaims(user).Result;
            var userClaims = _userManager.GetClaimsAsync(user).Result;
            if (userClaims.Any()) claims.AddRange(userClaims);
            
            context.IssuedClaims.AddRange(claims);
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var user = _userManager.GetUserAsync(context.Subject).Result;
            context.IsActive = user is {LockoutEnd: null};
            return Task.FromResult(0);
        }

        private Task<List<Claim>> InitializeDefaultClaims(UserEntity user)
        {
            var roles = _roleManager.Roles.AsNoTracking().ToListAsync().Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            
            // TODO: Improve hasura claims and add default claims
            var claims = new List<Claim>
            {
                new Claim(IdentityClaimDefaults.FullName, $"{user.FirstName} {user.LastName}"),
                new Claim(IdentityClaimDefaults.HasuraRole, GetHighestOrderRole(roles, userRoles)),
                new Claim(IdentityClaimDefaults.HasuraDefaultRole,IdentityRoleDefaults.User),
                new Claim(IdentityClaimDefaults.HasuraAllowedRoles, JsonSerializer.Serialize(roles.Select(r => r.Name).ToArray())),
            };
            return Task.FromResult(claims);
        }

        private string GetHighestOrderRole(IList<RoleEntity> roles, IList<string> userRoles)
        {
            var min = (2 * roles.Count);
            var highestOrderRole = "User";
            foreach (var role in roles)
            {
                if ((int) role.Level <= min && userRoles.Any(p => p.Equals(role.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    highestOrderRole = role.Name;
                    min = (int)role.Level;
                }
            }
            return highestOrderRole;
        }
    }
}
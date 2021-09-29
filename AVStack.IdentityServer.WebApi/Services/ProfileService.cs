using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Common.Constants;
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
            context.IssuedClaims.AddRange(InitializeDefaultClaims(user).Result);
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var user = _userManager.GetUserAsync(context.Subject).Result;
            context.IsActive = user is { LockoutEnd: null };
            return Task.FromResult(0);
        }

        private Task<List<Claim>> InitializeDefaultClaims(UserEntity user)
        {
            var systemRoles = _roleManager.Roles.AsNoTracking().ToListAsync().Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            
            // TODO: Improve hasura claims handling
            var claims = new List<Claim>
            {
                // TODO: Implement support for more then one role assigned to user 
                // new Claim(IdentityClaimDefaults.HasuraRole, GetHighestUserRole(systemRoles, userRoles)),
                new Claim(IdentityClaimDefaults.HasuraRole, userRoles.First()),
                new Claim(IdentityClaimDefaults.HasuraDefaultRole,IdentityRoleDefaults.User),
                new Claim(IdentityClaimDefaults.HasuraAllowedRoles, JsonSerializer.Serialize(systemRoles.Select(r => r.Name).ToArray())),
            };
            return Task.FromResult(claims);
        }

        private string GetHighestUserRole(ICollection<RoleEntity> roles, IList<string> userRoles)
        {
            var min = (2 * roles.Count);
            var highestOrderRole = IdentityRoleDefaults.User;
            foreach (var role in roles)
            {
                if ((int) role.Level > min || userRoles.Any(p => p.Equals(role.Name, StringComparison.InvariantCultureIgnoreCase))) continue;
                
                highestOrderRole = role.Name;
                min = (int)role.Level;
            }
            return highestOrderRole;
        }
    }
}
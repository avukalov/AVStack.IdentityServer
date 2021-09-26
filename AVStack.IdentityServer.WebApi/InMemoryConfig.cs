using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

namespace AVStack.IdentityServer.WebApi
{
    public static class InMemoryConfig
    {
        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        
        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
                new()
                {
                    ClientName = "AVStack.Angular",
                    ClientId = "AVStack.Angular",
                    ClientSecrets = new List<Secret>() {new Secret("AVStack.Angular".Sha256())},
                    RedirectUris = new List<string>() {"http://localhost:4200/signin-callback", "http://localhost:4200/assets/silent-callback.html" },
                    RequirePkce = true,
                    AllowAccessTokensViaBrowser = true,
                    AllowedGrantTypes = new List<string>()
                    {
                        GrantType.AuthorizationCode,
                        // GrantType.ClientCredentials,
                        // GrantType.ResourceOwnerPassword,
                    },
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    },
                    AllowedCorsOrigins = { "http://localhost:4200" },
                    RequireClientSecret = false,
                    PostLogoutRedirectUris = new List<string>() {"http://localhost:4200/signout-callback"},
                    RequireConsent = false,
                    AccessTokenLifetime = 600
                },
            };
    }
}
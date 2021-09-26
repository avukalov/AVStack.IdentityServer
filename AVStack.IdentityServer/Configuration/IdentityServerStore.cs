using System.Collections;
using System.Collections.Generic;
using IdentityServer4.Models;

namespace AVStack.IdentityServer.Configuration
{
    public class IdentityServerStore
    {
        public const string IdentityServerStoreSection = "IdentityServerStore";
        public IEnumerable<Client> Clients { get; set; }
        public IEnumerable<IdentityResource> IdentityResources { get; set; }
        public IEnumerable<ApiResource> ApiResources { get; set; }
        public IEnumerable<ApiScope> ApiScopes { get; set; }

    }
}
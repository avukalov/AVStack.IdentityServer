using System;

namespace AVStack.IdentityServer.WebApi.Data.Entities
{
    public class UserLoginHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime LastLogin { get; set; }
        public string IdAddress { get; set; }
    }
}
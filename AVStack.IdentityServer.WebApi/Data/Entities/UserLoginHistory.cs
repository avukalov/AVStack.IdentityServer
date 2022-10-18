using System;

namespace AVStack.IdentityServer.WebApi.Data.Entities
{
    public class UserLoginHistory : EntityBase
    {
        public Guid UserId { get; set; }
        public DateTime LoggedInAt { get; set; }
        public string LoggedInFrom { get; set; }
    }
}
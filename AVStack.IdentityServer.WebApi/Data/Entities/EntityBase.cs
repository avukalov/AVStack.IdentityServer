using System;

namespace AVStack.IdentityServer.WebApi.Data.Entities
{
    public class EntityBase
    {
        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
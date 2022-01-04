using System;
using AVStack.IdentityServer.WebApi.Models.Enums;

namespace AVStack.IdentityServer.WebApi.Data.Entities
{
    public class UserTempToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Value { get; set; }
        public TokenType TokenType { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
using System;

namespace AVStack.IdentityServer.WebApi.Models.Requests
{
    public class ChangeUserPasswordRequest
    {
        public Guid UserId { get; set; }
        
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
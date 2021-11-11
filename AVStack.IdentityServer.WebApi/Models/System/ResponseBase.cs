using System.Collections.Generic;
using AVStack.IdentityServer.WebApi.Models.System.Interfaces;

namespace AVStack.IdentityServer.WebApi.Models.System
{
    public abstract class ResponseBase : IResponse
    {
        public int StatusCode { get; set; }
        public bool Succeeded { get; set; } = false;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
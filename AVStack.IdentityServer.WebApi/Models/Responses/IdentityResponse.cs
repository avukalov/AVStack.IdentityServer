using System.Collections.Generic;
using System.Net;

namespace AVStack.IdentityServer.WebApi.Models.Responses
{
    public class IdentityResponse
    {
        public string Title { get; set; }
        public HttpStatusCode Status { get; set; }
        public bool Succeeded { get; set; } = true;
        public string Message { get; set; }

        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}
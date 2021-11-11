using System.Collections.Generic;

namespace AVStack.IdentityServer.WebApi.Models.System.Interfaces
{
    public interface IResponse
    {
        int StatusCode { get; set; }
        bool Succeeded { get; set; }
        List<string> Errors { get; set; } // TODO: Consider about adding error codes and change data type to dictionary
    }
}
using AVStack.IdentityServer.WebApi.Models.Requests;

namespace AVStack.IdentityServer.WebApi.Controllers
{
    public class RegisterInputModel : SignUpRequest
    {
        public bool ShowPassword { get; set; } = false;
    }
}
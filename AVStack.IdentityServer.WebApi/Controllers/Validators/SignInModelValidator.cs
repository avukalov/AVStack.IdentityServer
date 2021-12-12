using AVStack.IdentityServer.WebApi.Models.Requests;
using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class SignInModelValidator : AbstractValidator<SignInRequest>
    {
        public SignInModelValidator()
        {
            RuleFor(model => model.Email).NotEmpty().WithMessage("Email is required.");
            RuleFor(model => model.Password).NotEmpty().WithMessage("Password is required.");
        }
    }
}
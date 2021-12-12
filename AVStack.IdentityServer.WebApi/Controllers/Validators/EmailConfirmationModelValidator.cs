using AVStack.IdentityServer.WebApi.Models.Requests;
using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class EmailConfirmationModelValidator : AbstractValidator<EmailConfirmationRequest>
    {
        public EmailConfirmationModelValidator()
        {
            RuleFor(p => p.UserId).NotNull().WithMessage("UserId is required");
            RuleFor(p => p.Token).NotEmpty().WithMessage("Token is required");
        }
    }
}
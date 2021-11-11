using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class EmailConfirmationModelValidator : AbstractValidator<EmailConfirmationModel>
    {
        public EmailConfirmationModelValidator()
        {
            RuleFor(p => p.UserId).NotNull().WithMessage("UserId is required");
            RuleFor(p => p.Token).NotEmpty().WithMessage("Token is required");
        }
    }
}
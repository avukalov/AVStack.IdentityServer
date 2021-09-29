using FluentValidation;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class SignUpModelValidator : AbstractValidator<SignUpModel>
    {
        public SignUpModelValidator()
        {
            RuleFor(model => model.FirstName).NotEmpty().WithMessage("Firstname is required.");
            RuleFor(model => model.LastName).NotEmpty().WithMessage("Lastname is required.");
            
            RuleFor(model => model.Email).NotEmpty().WithMessage("Email is required.");
            RuleFor(model => model.Email).EmailAddress().WithMessage("Invalid email format.");
            
            RuleFor(model => model.Password).NotEmpty().WithMessage("Password is required.");
            RuleFor(model => model.Password).Equal(model => model.ConfirmPassword)
                .WithMessage("The password and confirmation password do not match.");
        }
    }
}
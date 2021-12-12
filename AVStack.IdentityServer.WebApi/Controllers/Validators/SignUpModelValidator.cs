using AVStack.IdentityServer.WebApi.Models.Requests;
using FluentValidation;
using System;

namespace AVStack.IdentityServer.WebApi.Controllers.Validators
{
    public class SignUpModelValidator : AbstractValidator<SignUpRequest>
    {
        public SignUpModelValidator()
        {
            RuleFor(model => model.FirstName).NotEmpty().WithMessage("Firstname is required.");
            RuleFor(model => model.LastName).NotEmpty().WithMessage("Lastname is required.");

            RuleFor(model => model.Email).NotEmpty().WithMessage("Email is required.");
            RuleFor(model => model.Email).EmailAddress().WithMessage("Invalid email format.");

            RuleFor(model => model.Password).NotEmpty().WithMessage("Password is required.");
            RuleFor(model => model.Password)
                .Equal(model => model.ConfirmPassword)
                .WithMessage("The password and confirmation password do not match.");
        }

        private readonly Func<SignUpRequest, string> _isUsernameValid = (model) =>
        {
            if (string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.UserName))
                return string.Empty;

            return model.UserName;
        };

        private readonly Func<SignUpRequest, string> _isEmailValid = (model) =>
        {
            if (string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.UserName))
                return string.Empty;

            return model.Email;
        };
    }
}
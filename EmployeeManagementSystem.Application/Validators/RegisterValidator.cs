using EmployeeManagementSystem.Domain.DTOs;
using FluentValidation;

namespace EmployeeManagementSystem.Application.Validators
{
    public class RegisterValidator : AbstractValidator<Register>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("The email address is not valid.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Must be at least 6 characters long.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MinimumLength(5).WithMessage("Must be at least 5 characters long.")
                .MaximumLength(100).WithMessage("Cannot be longer than 100 characters.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Must match with the password above.");
        }
    }
}

using EmployeeManagementSystem.Domain.DTOs;
using FluentValidation;

namespace EmployeeManagementSystem.Application.Validators
{
    public class AccountBaseValidator : AbstractValidator<AccountBase>
    {
        public AccountBaseValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("The email address is not valid.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Must be at least 6 characters long.");
        }
    }
}

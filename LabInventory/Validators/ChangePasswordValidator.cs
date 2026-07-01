using FluentValidation;
using LabInventory.Models.DTOs.Auth;

namespace LabInventory.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters.")
                .Matches("[A-Z]").WithMessage("Must contain at least one uppercase letter.")
                .Matches("[0-9]").WithMessage("Must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Must contain at least one special character.");
        }
    }
}
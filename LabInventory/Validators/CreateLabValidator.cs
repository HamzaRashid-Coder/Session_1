using FluentValidation;
using LabInventory.Models.DTOs.Labs;

namespace LabInventory.Validators
{
    public class CreateLabValidator : AbstractValidator<CreateLabDto>
    {
        public CreateLabValidator()
        {
            RuleFor(x => x.LabName)
                .NotEmpty().WithMessage("Lab name is required.")
                .MaximumLength(150).WithMessage("Lab name cannot exceed 150 characters.");

            RuleFor(x => x.Location)
                .MaximumLength(250).WithMessage("Location cannot exceed 250 characters.")
                .When(x => x.Location != null);
        }
    }
}
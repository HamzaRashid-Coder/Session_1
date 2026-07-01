using FluentValidation;
using LabInventory.Models.DTOs.Issuance;

namespace LabInventory.Validators
{
    public class ReturnItemValidator : AbstractValidator<ReturnItemDto>
    {
        public ReturnItemValidator()
        {
            RuleFor(x => x.ReturnDate)
                .NotEmpty().WithMessage("Return date is required.");

            RuleFor(x => x.Condition)
                .NotEmpty().WithMessage("Condition is required.")
                .Must(c => c == "Good" || c == "Broken" || c == "Lost")
                .WithMessage("Condition must be Good, Broken, or Lost.");
        }
    }
}
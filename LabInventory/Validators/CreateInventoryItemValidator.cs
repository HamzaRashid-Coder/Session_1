using FluentValidation;
using LabInventory.Models.DTOs.Inventory;

namespace LabInventory.Validators
{
    public class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemDto>
    {
        public CreateInventoryItemValidator()
        {
            RuleFor(x => x.LabId)
                .GreaterThan(0).WithMessage("A valid lab must be selected.");

            RuleFor(x => x.EquipmentName)
                .NotEmpty().WithMessage("Equipment name is required.")
                .MaximumLength(200).WithMessage("Equipment name cannot exceed 200 characters.");

            RuleFor(x => x.FinePerDay)
                .GreaterThanOrEqualTo(0).WithMessage("Fine per day cannot be negative.");

            RuleFor(x => x.TotalQuantity)
                .GreaterThan(0).WithMessage("Total quantity must be at least 1.");

            RuleFor(x => x.ModelNumber)
                .MaximumLength(100).WithMessage("Model number cannot exceed 100 characters.")
                .When(x => x.ModelNumber != null);
        }
    }
}
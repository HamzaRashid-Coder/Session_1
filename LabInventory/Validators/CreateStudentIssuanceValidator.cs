using FluentValidation;
using LabInventory.Models.DTOs.Issuance;

namespace LabInventory.Validators
{
    public class CreateStudentIssuanceValidator : AbstractValidator<CreateStudentIssuanceDto>
    {
        public CreateStudentIssuanceValidator()
        {
            RuleFor(x => x.LabId)
                .GreaterThan(0).WithMessage("A valid lab must be selected.");

            RuleFor(x => x.ItemId)
                .GreaterThan(0).WithMessage("A valid inventory item must be selected.");

            RuleFor(x => x.QuantityIssued)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");

            RuleFor(x => x.Student1Name)
                .NotEmpty().WithMessage("Primary student name is required.")
                .MaximumLength(150).WithMessage("Student name cannot exceed 150 characters.");

            RuleFor(x => x.RegistrationNo1)
                .NotEmpty().WithMessage("Primary student registration number is required.");

            RuleFor(x => x.IssueDate)
                .NotEmpty().WithMessage("Issue date is required.");

            // If DueDate is provided it must be after or equal to IssueDate
            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(x => x.IssueDate)
                .WithMessage("Due date must be on or after the issue date.")
                .When(x => x.DueDate.HasValue);

            // Student 2: if name given, reg no is required
            RuleFor(x => x.RegistrationNo2)
                .NotEmpty().WithMessage("Registration number is required for Student 2.")
                .When(x => !string.IsNullOrWhiteSpace(x.Student2Name));

            // Student 3: if name given, reg no is required
            RuleFor(x => x.RegistrationNo3)
                .NotEmpty().WithMessage("Registration number is required for Student 3.")
                .When(x => !string.IsNullOrWhiteSpace(x.Student3Name));
        }
    }
}
namespace LabInventory.Services.Interfaces
{
    public interface IFineCalculationService
    {
        decimal Calculate(DateOnly? dueDate, DateOnly returnDate, decimal finePerDay, int quantity);
    }
}

using Microsoft.EntityFrameworkCore;
using LabInventory.Services.Interfaces;

namespace LabInventory.Services.Implementations
{
    public class FineCalculationService : IFineCalculationService
    {
        public decimal Calculate(DateOnly? dueDate, DateOnly returnDate, decimal finePerDay, int quantity)
        {
            if (dueDate is null)
                return 0;

            if (returnDate <= dueDate.Value)
                return 0;

            int daysLate = returnDate.DayNumber - dueDate.Value.DayNumber;
            return daysLate * finePerDay * quantity;
        }
    }
}

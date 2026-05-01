// Models/ExpenseItem.cs
namespace FinanceApi.Models;

public class ExpenseItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  // "Обед в столовой"
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool IsActive { get; set; } = true;  // Активная

    // Navigation property
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
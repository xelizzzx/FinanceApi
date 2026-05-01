// Models/Category.cs
namespace FinanceApi.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  // "Кафе и рестораны"
    public decimal MonthlyBudget { get; set; }  // Бюджет на месяц
    public bool IsActive { get; set; } = true;  // Активная

    // Navigation property
    public ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();
}
// Models/Transaction.cs
namespace FinanceApi.Models;

public class Transaction
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; }  // Дата без времени
    public decimal Amount { get; set; }  // Сумма (положительное число)
    public string Comment { get; set; } = string.Empty;  // Комментарий
    public int ExpenseItemId { get; set; }
    public ExpenseItem? ExpenseItem { get; set; }
}
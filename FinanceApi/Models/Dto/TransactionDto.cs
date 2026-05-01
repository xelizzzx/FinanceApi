// Models/Dto/TransactionDto.cs
namespace FinanceApi.Models.Dto;

public class TransactionDto
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int ExpenseItemId { get; set; }
    public string? ExpenseItemName { get; set; }
    public string? CategoryName { get; set; }
}

public class CreateTransactionDto
{
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int ExpenseItemId { get; set; }
}
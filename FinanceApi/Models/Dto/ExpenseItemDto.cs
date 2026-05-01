// Models/Dto/ExpenseItemDto.cs
namespace FinanceApi.Models.Dto;

public class ExpenseItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
}

public class CreateExpenseItemDto
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateExpenseItemDto
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public bool IsActive { get; set; }
}
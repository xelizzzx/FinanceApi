// Models/Dto/CategoryDto.cs
namespace FinanceApi.Models.Dto;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyBudget { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyBudget { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyBudget { get; set; }
    public bool IsActive { get; set; }
}
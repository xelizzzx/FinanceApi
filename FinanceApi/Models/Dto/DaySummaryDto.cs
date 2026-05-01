// Models/Dto/DaySummaryDto.cs
namespace FinanceApi.Models.Dto;

public class DaySummaryDto
{
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public string StickerColor { get; set; } = string.Empty;  // "green", "yellow", "red"
    public string StickerMessage { get; set; } = string.Empty;
    public List<TransactionDto> Transactions { get; set; } = new();
}
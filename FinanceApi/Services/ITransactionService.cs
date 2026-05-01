// Services/ITransactionService.cs
using FinanceApi.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Services;

public interface ITransactionService
{
    Task<decimal> GetDayTotalAsync(DateTime date);
    Task ValidateDailyLimitAsync(DateTime date, decimal additionalAmount);
}



public class TransactionService : ITransactionService
{
    private readonly FinanceDbContext _context;
    private const decimal MAX_DAILY_AMOUNT = 1_000_000m;

    public TransactionService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetDayTotalAsync(DateTime date)
    {
        var targetDate = date.Date;

        return await _context.Transactions
            .Where(t => t.TransactionDate.Date == targetDate)
            .SumAsync(t => t.Amount);
    }

    public async Task ValidateDailyLimitAsync(DateTime date, decimal additionalAmount)
    {
        var currentTotal = await GetDayTotalAsync(date);
        var newTotal = currentTotal + additionalAmount;

        if (newTotal > MAX_DAILY_AMOUNT)
        {
            throw new InvalidOperationException(
                $"Превышен дневной лимит трат. " +
                $"Текущая сумма за день: {currentTotal} руб. " +
                $"Попытка добавить: {additionalAmount} руб. " +
                $"Максимальная сумма в день: {MAX_DAILY_AMOUNT} руб."
            );
        }
    }
}
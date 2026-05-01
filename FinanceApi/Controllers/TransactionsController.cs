// Controllers/TransactionsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApi.Data;
using FinanceApi.Models;
using FinanceApi.Models.Dto;
using FinanceApi.Services;

namespace FinanceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly FinanceDbContext _context;
    private readonly ITransactionService _transactionService;

    public TransactionsController(FinanceDbContext context, ITransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    /// <summary>
    /// Получить все транзакции
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TransactionDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _context.Transactions
            .Include(t => t.ExpenseItem)
            .ThenInclude(e => e!.Category)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                TransactionDate = t.TransactionDate,
                Amount = t.Amount,
                Comment = t.Comment,
                ExpenseItemId = t.ExpenseItemId,
                ExpenseItemName = t.ExpenseItem != null ? t.ExpenseItem.Name : null,
                CategoryName = t.ExpenseItem != null && t.ExpenseItem.Category != null
                    ? t.ExpenseItem.Category.Name : null
            })
            .ToListAsync();

        return Ok(transactions);
    }

    /// <summary>
    /// Получить транзакции за конкретный день
    /// </summary>
    [HttpGet("by-date")]
    [ProducesResponseType(typeof(List<TransactionDto>), 200)]
    public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
    {
        var targetDate = date.Date;

        var transactions = await _context.Transactions
            .Include(t => t.ExpenseItem)
            .ThenInclude(e => e!.Category)
            .Where(t => t.TransactionDate.Date == targetDate)
            .OrderBy(t => t.TransactionDate)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                TransactionDate = t.TransactionDate,
                Amount = t.Amount,
                Comment = t.Comment,
                ExpenseItemId = t.ExpenseItemId,
                ExpenseItemName = t.ExpenseItem != null ? t.ExpenseItem.Name : null,
                CategoryName = t.ExpenseItem != null && t.ExpenseItem.Category != null
                    ? t.ExpenseItem.Category.Name : null
            })
            .ToListAsync();

        return Ok(transactions);
    }

    /// <summary>
    /// Получить транзакции за месяц
    /// </summary>
    [HttpGet("by-month")]
    [ProducesResponseType(typeof(List<TransactionDto>), 200)]
    public async Task<IActionResult> GetByMonth([FromQuery] int year, [FromQuery] int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var transactions = await _context.Transactions
            .Include(t => t.ExpenseItem)
            .ThenInclude(e => e!.Category)
            .Where(t => t.TransactionDate.Date >= startDate && t.TransactionDate.Date <= endDate)
            .OrderBy(t => t.TransactionDate)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                TransactionDate = t.TransactionDate,
                Amount = t.Amount,
                Comment = t.Comment,
                ExpenseItemId = t.ExpenseItemId,
                ExpenseItemName = t.ExpenseItem != null ? t.ExpenseItem.Name : null,
                CategoryName = t.ExpenseItem != null && t.ExpenseItem.Category != null
                    ? t.ExpenseItem.Category.Name : null
            })
            .ToListAsync();

        return Ok(transactions);
    }

    /// <summary>
    /// Получить сводку по дню с цветным стикером (доп. функционал)
    /// </summary>
    [HttpGet("day-summary")]
    [ProducesResponseType(typeof(DaySummaryDto), 200)]
    public async Task<IActionResult> GetDaySummary([FromQuery] DateTime date)
    {
        var targetDate = date.Date;

        var transactions = await _context.Transactions
            .Include(t => t.ExpenseItem)
            .Where(t => t.TransactionDate.Date == targetDate)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                TransactionDate = t.TransactionDate,
                Amount = t.Amount,
                Comment = t.Comment,
                ExpenseItemId = t.ExpenseItemId,
                ExpenseItemName = t.ExpenseItem != null ? t.ExpenseItem.Name : null,
                CategoryName = t.ExpenseItem != null && t.ExpenseItem.Category != null
                ? t.ExpenseItem.Category.Name
                : null
            })
            .ToListAsync();

        var totalAmount = transactions.Sum(t => t.Amount);

        var (stickerColor, stickerMessage) = GetStickerInfo(totalAmount);

        return Ok(new DaySummaryDto
        {
            Date = targetDate,
            TotalAmount = totalAmount,
            StickerColor = stickerColor,
            StickerMessage = stickerMessage,
            Transactions = transactions
        });
    }

    /// <summary>
    /// Создать новую транзакцию
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        // Проверка положительной суммы
        if (dto.Amount <= 0)
            return BadRequest("Сумма должна быть положительным числом");

        // Проверка что статья расхода существует и активна
        var expenseItem = await _context.ExpenseItems
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == dto.ExpenseItemId);

        if (expenseItem == null)
            return BadRequest($"Статья расхода с ID {dto.ExpenseItemId} не найдена");

        if (!expenseItem.IsActive)
            return BadRequest("Нельзя выбрать неактивную статью расхода");

        // Проверка дневного лимита
        await _transactionService.ValidateDailyLimitAsync(dto.TransactionDate, dto.Amount);

        var transaction = new Transaction
        {
            TransactionDate = dto.TransactionDate.Date,
            Amount = dto.Amount,
            Comment = dto.Comment,
            ExpenseItemId = dto.ExpenseItemId
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, new TransactionDto
        {
            Id = transaction.Id,
            TransactionDate = transaction.TransactionDate,
            Amount = transaction.Amount,
            Comment = transaction.Comment,
            ExpenseItemId = transaction.ExpenseItemId,
            ExpenseItemName = expenseItem.Name,
            CategoryName = expenseItem.Category?.Name
        });
    }

    /// <summary>
    /// Получить транзакцию по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.ExpenseItem)
            .ThenInclude(e => e!.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return NotFound();

        return Ok(new TransactionDto
        {
            Id = transaction.Id,
            TransactionDate = transaction.TransactionDate,
            Amount = transaction.Amount,
            Comment = transaction.Comment,
            ExpenseItemId = transaction.ExpenseItemId,
            ExpenseItemName = transaction.ExpenseItem?.Name,
            CategoryName = transaction.ExpenseItem?.Category?.Name
        });
    }

    /// <summary>
    /// Удалить транзакцию
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
            return NotFound();

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private (string Color, string Message) GetStickerInfo(decimal totalAmount)
    {
        if (totalAmount < 500)
            return ("green", $"Траты {totalAmount} руб. — день прошёл экономно! 🟢");

        if (totalAmount <= 2000)
            return ("yellow", $"Траты {totalAmount} руб. — в пределах обычного. 🟡");

        return ("red", $"Траты {totalAmount} руб. — день был затратным! 🔴");
    }
}
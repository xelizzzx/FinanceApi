// Controllers/ExpenseItemsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApi.Data;
using FinanceApi.Models;
using FinanceApi.Models.Dto;

namespace FinanceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpenseItemsController : ControllerBase
{
    private readonly FinanceDbContext _context;

    public ExpenseItemsController(FinanceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список всех статей расходов
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ExpenseItemDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _context.ExpenseItems
            .Include(e => e.Category)
            .Select(e => new ExpenseItemDto
            {
                Id = e.Id,
                Name = e.Name,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                IsActive = e.IsActive
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Получить статью расхода по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExpenseItemDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _context.ExpenseItems
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (item == null)
            return NotFound();

        return Ok(new ExpenseItemDto
        {
            Id = item.Id,
            Name = item.Name,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name,
            IsActive = item.IsActive
        });
    }

    /// <summary>
    /// Создать новую статью расхода
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseItemDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Название статьи расхода обязательно");

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            return BadRequest($"Категория с ID {dto.CategoryId} не найдена");

        var item = new ExpenseItem
        {
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive
        };

        _context.ExpenseItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, new ExpenseItemDto
        {
            Id = item.Id,
            Name = item.Name,
            CategoryId = item.CategoryId,
            CategoryName = category.Name,
            IsActive = item.IsActive
        });
    }

    /// <summary>
    /// Обновить статью расхода
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseItemDto dto)
    {
        var item = await _context.ExpenseItems.FindAsync(id);
        if (item == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Название статьи расхода обязательно");

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            return BadRequest($"Категория с ID {dto.CategoryId} не найдена");

        item.Name = dto.Name;
        item.CategoryId = dto.CategoryId;
        item.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Удалить статью расхода
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.ExpenseItems
            .Include(e => e.Transactions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (item == null)
            return NotFound();

        if (item.Transactions.Any())
            return BadRequest("Нельзя удалить статью расхода, по которой есть транзакции");

        _context.ExpenseItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
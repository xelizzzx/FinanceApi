// Data/FinanceDbContext.cs
using FinanceApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FinanceApi.Data;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<ExpenseItem> ExpenseItems { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Устанавливаем точность для decimal (важно для денег)
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Category>()
            .Property(c => c.MonthlyBudget)
            .HasPrecision(18, 2);

        // Индекс для быстрого поиска по дате
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.TransactionDate);

        // Запрещаем каскадное удаление
        modelBuilder.Entity<ExpenseItem>()
            .HasMany(e => e.Transactions)
            .WithOne(t => t.ExpenseItem)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.ExpenseItems)
            .WithOne(e => e.Category)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
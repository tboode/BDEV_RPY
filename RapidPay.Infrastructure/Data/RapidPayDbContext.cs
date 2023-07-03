using Microsoft.EntityFrameworkCore;
using RapidPay.Core.Entities;

namespace RapidPay.Infrastructure.Data;

public class RapidPayDbContext: DbContext
{
    public virtual DbSet<Card> Cards { get; set; } = default!;
    
    private const string DB_PATH = "./rapidpay.db";
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DB_PATH}");
}
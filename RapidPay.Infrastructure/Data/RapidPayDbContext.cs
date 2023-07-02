using Microsoft.EntityFrameworkCore;
using RapidPay.Core.Entities;

namespace RapidPay.Infrastructure.Data;

public class RapidPayDbContext: DbContext
{
    public DbSet<Card> Cards { get; set; }
    public DbSet<User> Users { get; set; }
    
    private const string DB_PATH = "./rapidpay.db";
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DB_PATH}");

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<User>().HasData(new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000001") });
}
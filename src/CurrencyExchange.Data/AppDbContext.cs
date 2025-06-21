using CurrencyExchange.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var modelExhangeRateBuilder =modelBuilder.Entity<ExchangeRate>();
            modelExhangeRateBuilder.Property(e => e.Rate)
                .HasColumnType("decimal(18,4)");

            modelExhangeRateBuilder.Property(e => e.CreatedDate)
                .HasColumnType("DATE");

            modelExhangeRateBuilder.Property(e => e.UpdatedDate)
                .HasColumnType("DATETIME2")
                .IsRequired(false); 

            modelExhangeRateBuilder.HasIndex(e => new { e.Currency, e.CreatedDate })
                .IsUnique();

            modelBuilder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        }
    }
}

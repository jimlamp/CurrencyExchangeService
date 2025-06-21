using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Set up the connection string here or read from your configuration
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=PaymentsDB;User Id=sa;Password=1234qwer!;TrustServerCertificate=true;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PropertyManagement.API.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Uses LocalDB for running migrations locally (dotnet ef / Package Manager Console)
            // The deployed app reads its connection string from App Service environment variables
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=PropertyManagementDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

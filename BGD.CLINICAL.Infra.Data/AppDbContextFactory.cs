using BGD.CLINICAL.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BGD.CLINICAL.Infra.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=BGD_Clinical;Trusted_Connection=True;TrustServerCertificate=True;");

        return new AppDbContext(optionsBuilder.Options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Invest.Data;

/// <summary>
/// Permet aux outils EF (dotnet ef ...) de créer un contexte au design-time.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InvestDbContext>
{
    public InvestDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InvestDbContext>()
            .UseSqlite("Data Source=invest.db")
            .Options;
        return new InvestDbContext(options);
    }
}

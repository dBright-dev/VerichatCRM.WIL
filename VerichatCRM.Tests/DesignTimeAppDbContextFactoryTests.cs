using Microsoft.EntityFrameworkCore;
using VerichatCRM.API;
using Xunit;

namespace VerichatCRM.Tests;

public sealed class DesignTimeAppDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_ConfiguresSqlServerWithoutStartingApiHost()
    {
        var factory = new DesignTimeAppDbContextFactory();

        using var context = factory.CreateDbContext([]);

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
    }
}

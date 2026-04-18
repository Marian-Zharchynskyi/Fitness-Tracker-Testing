using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Database;

public abstract class BaseDatabaseTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("fitness-tracker-test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected ApplicationDbContext Context { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_dbContainer.GetConnectionString());
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(dataSource)
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching(false)
            .Options;

        Context = new ApplicationDbContext(options);
        await Context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    protected async Task<int> SaveChangesAsync()
    {
        var result = await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return result;
    }
}

using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using Tests.Data;
using Xunit;

namespace Tests.Performance;

public abstract class PerformanceTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("fitness-tracker-perf")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected ApplicationDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_dbContainer.GetConnectionString());
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(dataSource)
            .UseSnakeCaseNamingConvention()
            .Options;

        Context = new ApplicationDbContext(options);
        await Context.Database.MigrateAsync();

        var seeder = new DatabaseSeeder(Context);
        await seeder.SeedAsync(userCount: 100, workoutsPerUser: 100);
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}

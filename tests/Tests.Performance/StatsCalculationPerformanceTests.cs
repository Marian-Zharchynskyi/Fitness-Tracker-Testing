using System.Diagnostics;
using Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Tests.Performance;

public class StatsCalculationPerformanceTests : PerformanceTestBase
{
    private readonly ITestOutputHelper _output;

    public StatsCalculationPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task CalculateStats_For100Workouts_ShouldCompleteInUnder500Ms()
    {
        var user = await Context.Users.FirstAsync();
        var stopwatch = Stopwatch.StartNew();

        var stats = await Context.Workouts
            .Where(w => w.UserId == user.Id)
            .GroupBy(w => w.UserId)
            .Select(g => new
            {
                TotalWorkouts = g.Count(),
                TotalCalories = g.Sum(w => w.CaloriesBurned ?? 0),
                AvgDuration = g.Average(w => w.DurationMinutes ?? 0)
            })
            .FirstOrDefaultAsync();

        stopwatch.Stop();
        _output.WriteLine($"Stats calculation took {stopwatch.ElapsedMilliseconds}ms");

        stats.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task CalculateStats_WithDateFilter_ShouldCompleteInUnder500Ms()
    {
        var user = await Context.Users.FirstAsync();
        var startDate = DateTime.UtcNow.AddDays(-90);
        var endDate = DateTime.UtcNow;

        var stopwatch = Stopwatch.StartNew();

        var stats = await Context.Workouts
            .Where(w => w.UserId == user.Id && w.Date >= startDate && w.Date <= endDate)
            .GroupBy(w => w.UserId)
            .Select(g => new
            {
                TotalWorkouts = g.Count(),
                TotalCalories = g.Sum(w => w.CaloriesBurned ?? 0),
                AvgDuration = g.Average(w => w.DurationMinutes ?? 0)
            })
            .FirstOrDefaultAsync();

        stopwatch.Stop();
        _output.WriteLine($"Filtered stats calculation took {stopwatch.ElapsedMilliseconds}ms");

        stats.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task CalculateStats_ForMultipleUsers_ShouldCompleteInUnder2Seconds()
    {
        var stopwatch = Stopwatch.StartNew();

        var allStats = await Context.Workouts
            .GroupBy(w => w.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalWorkouts = g.Count(),
                TotalCalories = g.Sum(w => w.CaloriesBurned ?? 0),
                AvgDuration = g.Average(w => w.DurationMinutes ?? 0)
            })
            .ToListAsync();

        stopwatch.Stop();
        _output.WriteLine($"Stats for {allStats.Count} users took {stopwatch.ElapsedMilliseconds}ms");

        allStats.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task GetExerciseProgress_ShouldCompleteInUnder1Second()
    {
        var user = await Context.Users.FirstAsync();
        var exerciseName = "Bench Press";

        var stopwatch = Stopwatch.StartNew();

        var progress = await Context.Workouts
            .Where(w => w.UserId == user.Id)
            .OrderBy(w => w.Date)
            .SelectMany(w => w.Exercises
                .Where(e => e.Name == exerciseName)
                .Select(e => new
                {
                    w.Date,
                    e.Sets,
                    e.Reps,
                    e.WeightKg,
                    e.DurationSeconds
                }))
            .ToListAsync();

        stopwatch.Stop();
        _output.WriteLine($"Exercise progress query took {stopwatch.ElapsedMilliseconds}ms, found {progress.Count} records");

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }
}

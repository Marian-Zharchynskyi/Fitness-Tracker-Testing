using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Users;
using Domain.Workouts;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class WorkoutRepository(ApplicationDbContext context) : IWorkoutRepository, IWorkoutQueries
{
    public async Task<Workout> Create(Workout workout, CancellationToken cancellationToken)
    {
        await context.Set<Workout>().AddAsync(workout, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return workout;
    }

    public async Task<Workout> Update(Workout workout, CancellationToken cancellationToken)
    {
        context.Set<Workout>().Update(workout);
        await context.SaveChangesAsync(cancellationToken);
        return workout;
    }

    public async Task<Workout> Delete(Workout workout, CancellationToken cancellationToken)
    {
        context.Set<Workout>().Remove(workout);
        await context.SaveChangesAsync(cancellationToken);
        return workout;
    }

    public async Task<Option<Workout>> GetById(WorkoutId id, CancellationToken cancellationToken)
    {
        var workout = await context.Set<Workout>()
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
            
        return workout == null ? Option.None<Workout>() : Option.Some(workout);
    }

    public async Task<IReadOnlyList<Workout>> GetByUserId(UserId userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var query = context.Set<Workout>().AsNoTracking()
            .Include(w => w.Exercises)
            .Where(w => w.UserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(w => w.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(w => w.Date <= endDate.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Workout>> GetAllByUserId(UserId userId, CancellationToken cancellationToken)
    {
        return await context.Set<Workout>().AsNoTracking()
            .Include(w => w.Exercises)
            .Where(w => w.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}

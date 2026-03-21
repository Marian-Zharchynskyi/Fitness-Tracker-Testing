using Domain.Users;
using Domain.Workouts;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IWorkoutQueries
{
    Task<Option<Workout>> GetById(WorkoutId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Workout>> GetByUserId(UserId userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    Task<IReadOnlyList<Workout>> GetAllByUserId(UserId userId, CancellationToken cancellationToken);
}

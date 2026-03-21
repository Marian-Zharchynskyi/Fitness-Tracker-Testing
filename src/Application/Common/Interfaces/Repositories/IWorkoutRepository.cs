using Domain.Workouts;
using Optional;

namespace Application.Common.Interfaces.Repositories;

public interface IWorkoutRepository
{
    Task<Workout> Create(Workout workout, CancellationToken cancellationToken);
    Task<Workout> Update(Workout workout, CancellationToken cancellationToken);
    Task<Workout> Delete(Workout workout, CancellationToken cancellationToken);
    Task<Option<Workout>> GetById(WorkoutId id, CancellationToken cancellationToken);
}

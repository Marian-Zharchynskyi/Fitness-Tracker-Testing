using Application.Common.Interfaces.Queries;
using Domain.Workouts;
using MediatR;
using Optional;

namespace Application.Workouts.Queries;

public record GetWorkoutQuery(Guid WorkoutId) : IRequest<Option<Workout>>;

public class GetWorkoutQueryHandler(IWorkoutQueries workoutQueries) : IRequestHandler<GetWorkoutQuery, Option<Workout>>
{
    public Task<Option<Workout>> Handle(GetWorkoutQuery request, CancellationToken cancellationToken)
    {
        return workoutQueries.GetById(new WorkoutId(request.WorkoutId), cancellationToken);
    }
}

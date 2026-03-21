using Application.Common.Interfaces.Queries;
using Domain.Users;
using Domain.Workouts;
using MediatR;

namespace Application.Workouts.Queries;

public record GetWorkoutsQuery(Guid UserId, DateTime? StartDate, DateTime? EndDate) : IRequest<IReadOnlyList<Workout>>;

public class GetWorkoutsQueryHandler(IWorkoutQueries workoutQueries) : IRequestHandler<GetWorkoutsQuery, IReadOnlyList<Workout>>
{
    public Task<IReadOnlyList<Workout>> Handle(GetWorkoutsQuery request, CancellationToken cancellationToken)
    {
        return workoutQueries.GetByUserId(new UserId(request.UserId), request.StartDate, request.EndDate, cancellationToken);
    }
}

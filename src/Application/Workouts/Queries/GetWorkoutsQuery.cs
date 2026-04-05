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
        var utcStartDate = request.StartDate.HasValue 
            ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc) 
            : (DateTime?)null;
        var utcEndDate = request.EndDate.HasValue 
            ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc) 
            : (DateTime?)null;
            
        return workoutQueries.GetByUserId(new UserId(request.UserId), utcStartDate, utcEndDate, cancellationToken);
    }
}

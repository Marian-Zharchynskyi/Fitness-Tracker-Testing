using Application.Common.Interfaces.Queries;
using Domain.Users;
using MediatR;

namespace Application.Users.Queries;

public record GetUserStatsQuery(Guid UserId, DateTime? StartDate, DateTime? EndDate) : IRequest<UserStatsResult>;

public class GetUserStatsQueryHandler(IWorkoutQueries workoutQueries) : IRequestHandler<GetUserStatsQuery, UserStatsResult>
{
    public async Task<UserStatsResult> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        var utcStartDate = request.StartDate.HasValue 
            ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc) 
            : (DateTime?)null;
        var utcEndDate = request.EndDate.HasValue 
            ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc) 
            : (DateTime?)null;
            
        var workouts = await workoutQueries.GetByUserId(new UserId(request.UserId), utcStartDate, utcEndDate, cancellationToken);

        var totalWorkouts = workouts.Count;
        var totalCalories = workouts.Sum(w => w.CaloriesBurned ?? 0);
        var averageDuration = workouts.Count > 0 
            ? workouts.Average(w => w.DurationMinutes ?? 0) 
            : 0;

        return new UserStatsResult(totalWorkouts, averageDuration, totalCalories);
    }
}

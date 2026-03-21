using Application.Common.Interfaces.Queries;
using Domain.Users;
using MediatR;

namespace Application.Users.Queries;

public record GetExerciseProgressQuery(Guid UserId, string ExerciseName) : IRequest<IReadOnlyList<ExerciseProgressResult>>;

public class GetExerciseProgressQueryHandler(IWorkoutQueries workoutQueries)
    : IRequestHandler<GetExerciseProgressQuery, IReadOnlyList<ExerciseProgressResult>>
{
    public async Task<IReadOnlyList<ExerciseProgressResult>> Handle(GetExerciseProgressQuery request, CancellationToken cancellationToken)
    {
        var workouts = await workoutQueries.GetAllByUserId(new UserId(request.UserId), cancellationToken);

        var progress = workouts
            .OrderBy(w => w.Date)
            .SelectMany(w => w.Exercises
                .Where(e => e.Name.Equals(request.ExerciseName, StringComparison.OrdinalIgnoreCase))
                .Select(e => new ExerciseProgressResult(
                    w.Date,
                    e.Sets,
                    e.Reps,
                    e.WeightKg,
                    e.DurationSeconds
                )))
            .ToList();

        return progress;
    }
}

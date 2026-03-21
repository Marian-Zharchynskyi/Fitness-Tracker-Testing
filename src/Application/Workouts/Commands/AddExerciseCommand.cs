using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Workouts.Exceptions;
using Domain.Workouts;
using MediatR;

namespace Application.Workouts.Commands;

public record AddExerciseCommand : IRequest<Result<Workout, WorkoutException>>
{
    public required Guid WorkoutId { get; init; }
    public required string Name { get; init; }
    public int? Sets { get; init; }
    public int? Reps { get; init; }
    public decimal? WeightKg { get; init; }
    public int? DurationSeconds { get; init; }
}

public class AddExerciseCommandHandler(IWorkoutRepository workoutRepository)
    : IRequestHandler<AddExerciseCommand, Result<Workout, WorkoutException>>
{
    public async Task<Result<Workout, WorkoutException>> Handle(
        AddExerciseCommand request,
        CancellationToken cancellationToken)
    {
        var workoutOpt = await workoutRepository.GetById(new WorkoutId(request.WorkoutId), cancellationToken);

        return await workoutOpt.Match(
            async w =>
            {
                try
                {
                    var exercise = Exercise.New(new ExerciseId(Guid.NewGuid()), w.Id, request.Name);

                    if (request.Sets.HasValue && request.Reps.HasValue)
                    {
                        exercise.SetMetrics(request.Sets.Value, request.Reps.Value, request.WeightKg, request.DurationSeconds);
                    }

                    w.AddExercise(exercise);
                    return await workoutRepository.Update(w, cancellationToken);
                }
                catch (ArgumentException ex)
                {
                    return new WorkoutException(ex.Message);
                }
                catch (Exception)
                {
                    return new WorkoutException("Unknown error occurred");
                }
            },
            () => Task.FromResult<Result<Workout, WorkoutException>>(new WorkoutException("Workout not found"))
        );
    }
}

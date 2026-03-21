using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Workouts.Exceptions;
using Domain.Workouts;
using MediatR;

namespace Application.Workouts.Commands;

public record UpdateWorkoutCommand : IRequest<Result<Workout, WorkoutException>>
{
    public required Guid WorkoutId { get; init; }
    public required string Name { get; init; }
    public string? Notes { get; init; }
    public DateTime? Date { get; init; }
    public int? DurationMinutes { get; init; }
    public int? CaloriesBurned { get; init; }
}

public class UpdateWorkoutCommandHandler(IWorkoutRepository workoutRepository)
    : IRequestHandler<UpdateWorkoutCommand, Result<Workout, WorkoutException>>
{
    public async Task<Result<Workout, WorkoutException>> Handle(
        UpdateWorkoutCommand request,
        CancellationToken cancellationToken)
    {
        var workoutOpt = await workoutRepository.GetById(new WorkoutId(request.WorkoutId), cancellationToken);

        return await workoutOpt.Match(
            async w =>
            {
                try
                {
                    w.UpdateDetails(request.Name, request.Notes);

                    if (request.Date.HasValue)
                    {
                        w.SetDate(request.Date.Value);
                    }

                    if (request.DurationMinutes.HasValue && request.CaloriesBurned.HasValue)
                    {
                        w.SetMetrics(request.DurationMinutes.Value, request.CaloriesBurned.Value);
                    }

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

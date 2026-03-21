using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Workouts.Exceptions;
using Domain.Users;
using Domain.Workouts;
using MediatR;

namespace Application.Workouts.Commands;

public record CreateWorkoutCommand : IRequest<Result<Workout, WorkoutException>>
{
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public string? Notes { get; init; }
    public DateTime? Date { get; init; }
    public int? DurationMinutes { get; init; }
    public int? CaloriesBurned { get; init; }
}

public class CreateWorkoutCommandHandler(IWorkoutRepository workoutRepository, IUserRepository userRepository)
    : IRequestHandler<CreateWorkoutCommand, Result<Workout, WorkoutException>>
{
    public async Task<Result<Workout, WorkoutException>> Handle(
        CreateWorkoutCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(new UserId(request.UserId), cancellationToken);

        return await user.Match(
            async u =>
            {
                try
                {
                    var workout = Workout.New(new WorkoutId(Guid.NewGuid()), u.Id, request.Name, request.Notes);

                    if (request.Date.HasValue)
                    {
                        workout.SetDate(request.Date.Value);
                    }

                    if (request.DurationMinutes.HasValue && request.CaloriesBurned.HasValue)
                    {
                        workout.SetMetrics(request.DurationMinutes.Value, request.CaloriesBurned.Value);
                    }

                    return await workoutRepository.Create(workout, cancellationToken);
                }
                catch (ArgumentException ex)
                {
                    return new WorkoutException(ex.Message);
                }
                catch (Exception ex)
                {
                    return new WorkoutException("Unknown error occurred");
                }
            },
            () => Task.FromResult<Result<Workout, WorkoutException>>(new WorkoutException("User not found"))
        );
    }
}

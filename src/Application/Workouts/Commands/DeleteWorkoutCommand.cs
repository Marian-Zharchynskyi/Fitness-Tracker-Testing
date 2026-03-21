using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Workouts.Exceptions;
using Domain.Workouts;
using MediatR;

namespace Application.Workouts.Commands;

public record DeleteWorkoutCommand : IRequest<Result<Workout, WorkoutException>>
{
    public required Guid WorkoutId { get; init; }
}

public class DeleteWorkoutCommandHandler(IWorkoutRepository workoutRepository)
    : IRequestHandler<DeleteWorkoutCommand, Result<Workout, WorkoutException>>
{
    public async Task<Result<Workout, WorkoutException>> Handle(
        DeleteWorkoutCommand request,
        CancellationToken cancellationToken)
    {
        var workoutOpt = await workoutRepository.GetById(new WorkoutId(request.WorkoutId), cancellationToken);

        return await workoutOpt.Match(
            async w =>
            {
                try
                {
                    return await workoutRepository.Delete(w, cancellationToken);
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

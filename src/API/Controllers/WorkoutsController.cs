using API.DTOs.Workouts;
using Application.Workouts.Commands;
using Application.Workouts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<WorkoutDto>> CreateWorkout([FromBody] CreateWorkoutDto request, CancellationToken ct)
    {
        var command = new CreateWorkoutCommand
        {
            UserId = request.UserId,
            Name = request.Name,
            Notes = request.Notes,
            Date = request.Date,
            DurationMinutes = request.DurationMinutes,
            CaloriesBurned = request.CaloriesBurned
        };

        var result = await sender.Send(command, ct);

        return result.Match<ActionResult<WorkoutDto>>(
            workout => Ok(WorkoutDto.FromDomainModel(workout)),
            error => BadRequest(new { error.Message })
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkoutDto>> GetWorkout(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetWorkoutQuery(id), ct);

        return result.Match<ActionResult<WorkoutDto>>(
            workout => Ok(WorkoutDto.FromDomainModel(workout)),
            () => NotFound()
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkoutDto>> UpdateWorkout(Guid id, [FromBody] UpdateWorkoutDto request, CancellationToken ct)
    {
        var command = new UpdateWorkoutCommand
        {
            WorkoutId = id,
            Name = request.Name,
            Notes = request.Notes,
            Date = request.Date,
            DurationMinutes = request.DurationMinutes,
            CaloriesBurned = request.CaloriesBurned
        };

        var result = await sender.Send(command, ct);

        return result.Match<ActionResult<WorkoutDto>>(
            workout => Ok(WorkoutDto.FromDomainModel(workout)),
            error => BadRequest(new { error.Message })
        );
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteWorkout(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteWorkoutCommand { WorkoutId = id }, ct);

        return result.Match<ActionResult>(
            _ => NoContent(),
            error => BadRequest(new { error.Message })
        );
    }

    [HttpPost("{id:guid}/exercises")]
    public async Task<ActionResult<WorkoutDto>> AddExercise(Guid id, [FromBody] AddExerciseDto request, CancellationToken ct)
    {
        var command = new AddExerciseCommand
        {
            WorkoutId = id,
            Name = request.Name,
            Sets = request.Sets,
            Reps = request.Reps,
            WeightKg = request.WeightKg,
            DurationSeconds = request.DurationSeconds
        };

        var result = await sender.Send(command, ct);

        return result.Match<ActionResult<WorkoutDto>>(
            workout => Ok(WorkoutDto.FromDomainModel(workout)),
            error => BadRequest(new { error.Message })
        );
    }
}

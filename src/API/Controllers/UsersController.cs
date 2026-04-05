using API.DTOs.Users;
using API.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Users.Commands;
using Domain.Users;
using MediatR;
using Application.Workouts.Queries;
using API.DTOs.Workouts;
using Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("users")]
[ApiController]
public class UsersController(ISender sender, IUserQueries userQueries) : ControllerBase
{
    [HttpGet("get-all")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await userQueries.GetAll(cancellationToken);
        return entities.Select(UserDto.FromDomainModel).ToList();
    }

    [HttpGet("get-by-id/{userId:guid}")]
    public async Task<ActionResult<UserDto>> Get([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var entity = await userQueries.GetById(new UserId(userId), cancellationToken);

        return entity.Match<ActionResult<UserDto>>(
            u => UserDto.FromDomainModel(u),
            () => NotFound());
    }

    [HttpPost("create")]
    public async Task<ActionResult<UserDto>> Create(
        [FromBody] CreateUserDto createUserDto,
        CancellationToken cancellationToken)
    {
        var input = new CreateUserCommand
        {
            Email = createUserDto.Email,
            Password = createUserDto.Password,
            Name = createUserDto.Name,
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<UserDto>>(
            user => UserDto.FromDomainModel(user),
            error => error.ToObjectResult());
    }

    [HttpPut("update/{userId:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUser(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserDto updateUserDto,
        CancellationToken cancellationToken)
    {
        var input = new UpdateUserCommand
        {
            UserId = userId,
            Email = updateUserDto.Email,
            Name = updateUserDto.Name,
            HeightCm = updateUserDto.HeightCm,
            WeightKg = updateUserDto.WeightKg
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<UserDto>>(
            u => UserDto.FromDomainModel(u),
            e => e.ToObjectResult());
    }

    [HttpDelete("delete/{userId:guid}")]
    public async Task<ActionResult<UserDto>> Delete([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var input = new DeleteUserCommand
        {
            UserId = userId
        };

        var result = await sender.Send(input, cancellationToken);

        return result.Match<ActionResult<UserDto>>(
            c => UserDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    [HttpGet("{id:guid}/workouts")]
    public async Task<ActionResult<IReadOnlyList<WorkoutDto>>> GetWorkouts(
        Guid id, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate, 
        CancellationToken ct)
    {
        var result = await sender.Send(new GetWorkoutsQuery(id, startDate, endDate), ct);
        return Ok(result.Select(WorkoutDto.FromDomainModel).ToList());
    }

    [HttpGet("{id:guid}/stats")]
    public async Task<ActionResult<UserStatsDto>> GetStats(
        Guid id, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate, 
        CancellationToken ct)
    {
        var result = await sender.Send(new GetUserStatsQuery(id, startDate, endDate), ct);
        return Ok(new UserStatsDto(result.TotalWorkouts, result.AverageDurationMinutes, result.TotalCaloriesBurned));
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<ActionResult<IReadOnlyList<ExerciseProgressDto>>> GetProgress(
        Guid id, 
        [FromQuery(Name = "exercise")] string exerciseName, 
        CancellationToken ct)
    {
        var result = await sender.Send(new GetExerciseProgressQuery(id, exerciseName), ct);
        return Ok(result.Select(r => new ExerciseProgressDto(r.Date, r.Sets, r.Reps, r.WeightKg, r.DurationSeconds)).ToList());
    }
}
using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Users;
using MediatR;

namespace Application.Users.Commands;

public record UpdateUserCommand : IRequest<Result<User, UserException>>
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public string? Name { get; init; }
    public decimal? HeightCm { get; init; }
    public decimal? WeightKg { get; init; }
}

public class UpdateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateUserCommand, Result<User, UserException>>
{
    public async Task<Result<User, UserException>> Handle(UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var existingUser = await userRepository.GetById(userId, cancellationToken);

        return await existingUser.Match(
            async u =>
            {
                var existingEmail =
                    await userRepository.SearchByEmailForUpdate(userId, request.Email, cancellationToken);

                return await existingEmail.Match(
                    e => Task.FromResult<Result<User, UserException>>(
                        new UserByThisEmailAlreadyExistsException(userId)),
                    async () => await UpdateEntity(u, request.Email, request.Name, request.HeightCm, request.WeightKg,
                        cancellationToken));
            },
            () => Task.FromResult<Result<User, UserException>>(
                new UserNotFoundException(userId)));
    }

    private async Task<Result<User, UserException>> UpdateEntity(
        User user,
        string email,
        string? name,
        decimal? heightCm,
        decimal? weightKg,
        CancellationToken cancellationToken)
    {
        try
        {
            user.UpdateUser(email, name);
            user.SetMetrics(heightCm, weightKg);
            return await userRepository.Update(user, cancellationToken);
        }
        catch (Exception exception)
        {
            return new UserUnknownException(user.Id, exception);
        }
    }
}

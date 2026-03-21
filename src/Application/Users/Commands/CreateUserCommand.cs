using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Users;
using MediatR;

namespace Application.Users.Commands;

public record CreateUserCommand : IRequest<Result<User, UserException>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? Name { get; init; }
}

public class CreateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<CreateUserCommand, Result<User, UserException>>
{
    public async Task<Result<User, UserException>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.SearchByEmail(request.Email, cancellationToken);

        return await existingUser.Match(
            _ => Task.FromResult<Result<User, UserException>>(
                new UserByThisEmailAlreadyExistsException(UserId.Empty)),
            async () => await CreateUser(request, cancellationToken));
    }

    private async Task<Result<User, UserException>> CreateUser(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Simple password hash — replace with a real hash service when ready
            var passwordHash = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(request.Password));

            var userId = UserId.New();
            var user = User.New(
                userId,
                request.Email,
                request.Name);

            return await userRepository.Create(user, cancellationToken);
        }
        catch (Exception exception)
        {
            return new UserUnknownException(UserId.Empty, exception);
        }
    }
}

using Domain.Users;

namespace API.DTOs.Users;

public record UserDto(
    Guid Id,
    string Email,
    string? Name,
    string? Surname,
    string? PhoneNumber,
    bool HasPassword)
{
    public static UserDto FromDomainModel(User user)
        => new(
            user.Id.Value,
            user.Email,
            user.Name,
            user.Surname,
            user.PhoneNumber,
            !string.IsNullOrEmpty(user.PasswordHash));
}
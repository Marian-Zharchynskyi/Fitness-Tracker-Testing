using Domain.Users;

namespace API.DTOs.Users;

public record UserDto(
    Guid Id,
    string Email,
    string? Name,
    decimal? HeightCm,
    decimal? WeightKg)
{
    public static UserDto FromDomainModel(User user)
        => new(
            user.Id.Value,
            user.Email,
            user.Name,
            user.HeightCm,
            user.WeightKg);
}
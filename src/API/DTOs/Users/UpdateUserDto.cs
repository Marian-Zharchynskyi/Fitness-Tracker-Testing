namespace API.DTOs.Users;

public record UpdateUserDto(
    string Email,
    string? Name,
    decimal? HeightCm,
    decimal? WeightKg);

namespace API.DTOs.Users;

public record UpdateUserDto(
    string Email,
    string? Name,
    string? Surname,
    string? PhoneNumber);

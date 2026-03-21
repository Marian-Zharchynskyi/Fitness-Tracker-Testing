namespace API.DTOs.Users;

public record CreateUserDto(
    string Email,
    string Password,
    string? Name);

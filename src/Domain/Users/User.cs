namespace Domain.Users;

public class User
{
    public UserId Id { get; }
    public string? ClerkId { get; private set; }
    public string Email { get; private set; }
    public string? Name { get; private set; }
    public string? Surname { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; }

    private User(UserId id, string? clerkId, string email, string? name, string? surname, string? phoneNumber, string passwordHash)
    {
        Id = id;
        ClerkId = clerkId;
        Email = email;
        Name = name;
        Surname = surname;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
    }

    public static User New(UserId id, string email, string? name, string? surname, string? phoneNumber, string passwordHash, string? clerkId = null)
        => new(id, clerkId, email, name, surname, phoneNumber, passwordHash);

    public void UpdateUser(string email, string? name, string? surname, string? phoneNumber)
    {
        Email = email;
        Name = name;
        Surname = surname;
        PhoneNumber = phoneNumber;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
namespace Domain.Users;

public class User
{
    public UserId Id { get; }
    public string Email { get; private set; }
    public string? Name { get; private set; }

    public decimal? HeightCm { get; private set; }
    public decimal? WeightKg { get; private set; }

    private User(UserId id, string email, string? name)
    {
        Id = id;
        Email = email;
        Name = name;
    }

    public static User New(UserId id, string email, string? name)
        => new(id, email, name);

    public void UpdateUser(string email, string? name)
    {
        Email = email;
        Name = name;
    }

    public void SetMetrics(decimal? heightCm, decimal? weightKg)
    {
        if (heightCm is <= 0)
        {
            throw new ArgumentException("Height must be positive", nameof(heightCm));
        }

        if (weightKg is <= 0)
        {
            throw new ArgumentException("Weight must be positive", nameof(weightKg));
        }

        HeightCm = heightCm;
        WeightKg = weightKg;
    }
}
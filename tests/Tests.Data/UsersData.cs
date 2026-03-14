using Domain.Users;

namespace Tests.Data;

public static class UsersData
{
    public static User MainUser => User.New(
        id: UserId.New(),
        email: "main.user@fitness.com",
        name: "John",
        surname: "Doe",
        phoneNumber: null,
        passwordHash: Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123")));

    public static User SecondUser => User.New(
        id: UserId.New(),
        email: "second.user@fitness.com",
        name: "Jane",
        surname: "Smith",
        phoneNumber: "+380991234567",
        passwordHash: Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password456")));
}

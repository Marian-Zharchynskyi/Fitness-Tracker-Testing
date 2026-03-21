using Domain.Users;

namespace Tests.Data;

public static class UsersData
{
    public static User MainUser => User.New(
        id: UserId.New(),
        email: "main.user@fitness.com",
        name: "John");

    public static User SecondUser => User.New(
        id: UserId.New(),
        email: "second.user@fitness.com",
        name: "Jane");
}

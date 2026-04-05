using Bogus;
using Domain.Users;
using Domain.Workouts;
using Infrastructure.Persistence;

namespace Tests.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly Faker _faker;

    public DatabaseSeeder(ApplicationDbContext context)
    {
        _context = context;
        _faker = new Faker();
    }

    public async Task SeedAsync(int userCount = 100, int workoutsPerUser = 100)
    {
        var users = GenerateUsers(userCount);
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var workouts = new List<Workout>();
        foreach (var user in users)
        {
            var userWorkouts = GenerateWorkoutsForUser(user.Id, workoutsPerUser);
            workouts.AddRange(userWorkouts);
        }

        _context.Workouts.AddRange(workouts);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();
    }

    private List<User> GenerateUsers(int count)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            var user = User.New(
                new UserId(Guid.NewGuid()),
                _faker.Internet.Email(),
                _faker.Name.FullName());

            if (_faker.Random.Bool(0.8f))
            {
                user.SetMetrics(
                    _faker.Random.Decimal(150, 200),
                    _faker.Random.Decimal(50, 120));
            }

            users.Add(user);
        }
        return users;
    }

    private List<Workout> GenerateWorkoutsForUser(UserId userId, int count)
    {
        var workouts = new List<Workout>();
        var exerciseNames = new[]
        {
            "Bench Press", "Squat", "Deadlift", "Pull-ups", "Push-ups",
            "Shoulder Press", "Barbell Row", "Lunges", "Plank", "Bicep Curls",
            "Tricep Dips", "Leg Press", "Lat Pulldown", "Cable Fly", "Leg Curls"
        };

        var workoutNames = new[]
        {
            "Morning Workout", "Evening Session", "Strength Training", "Cardio Day",
            "Full Body", "Upper Body", "Lower Body", "Core Workout", "HIIT Session",
            "Recovery Day"
        };

        for (int i = 0; i < count; i++)
        {
            var daysAgo = _faker.Random.Int(1, 365);
            var date = DateTime.UtcNow.AddDays(-daysAgo);

            var workout = Workout.New(
                new WorkoutId(Guid.NewGuid()),
                userId,
                _faker.PickRandom(workoutNames),
                _faker.Random.Bool(0.3f) ? _faker.Lorem.Sentence() : null);

            workout.SetDate(date);
            workout.SetMetrics(
                _faker.Random.Int(15, 120),
                _faker.Random.Int(100, 800));

            var exerciseCount = _faker.Random.Int(2, 6);
            for (int j = 0; j < exerciseCount; j++)
            {
                var exercise = Exercise.New(
                    new ExerciseId(Guid.NewGuid()),
                    workout.Id,
                    _faker.PickRandom(exerciseNames));

                var sets = _faker.Random.Int(2, 5);
                var reps = _faker.Random.Int(6, 15);
                var weight = _faker.Random.Bool(0.7f) ? _faker.Random.Decimal(5, 150) : (decimal?)null;
                var duration = _faker.Random.Bool(0.3f) ? _faker.Random.Int(30, 300) : (int?)null;

                exercise.SetMetrics(sets, reps, weight, duration);
                workout.AddExercise(exercise);
            }

            workouts.Add(workout);
        }

        return workouts;
    }
}

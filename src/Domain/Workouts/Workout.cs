using Domain.Users;

namespace Domain.Workouts;

public class Workout
{
    public WorkoutId Id { get; }
    public UserId UserId { get; private set; }
    public string Name { get; private set; }
    public string? Notes { get; private set; }
    
    public DateTime? Date { get; private set; }
    public int? DurationMinutes { get; private set; }
    public int? CaloriesBurned { get; private set; }

    private readonly List<Exercise> _exercises = [];
    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

    private Workout(WorkoutId id, UserId userId, string name, string? notes)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Notes = notes;
    }

    public static Workout New(WorkoutId id, UserId userId, string name, string? notes = null)
        => new(id, userId, name, notes);

    public void SetDate(DateTime date)
    {
        if (date > DateTime.UtcNow)
        {
            throw new ArgumentException("Workout date cannot be in the future", nameof(date));
        }
        Date = date;
    }

    public void SetMetrics(int durationMinutes, int caloriesBurned)
    {
        if (durationMinutes <= 0)
        {
            throw new ArgumentException("Duration must be positive", nameof(durationMinutes));
        }

        if (caloriesBurned <= 0)
        {
            throw new ArgumentException("Calories burned must be positive", nameof(caloriesBurned));
        }

        DurationMinutes = durationMinutes;
        CaloriesBurned = caloriesBurned;
    }

    public void UpdateDetails(string name, string? notes)
    {
        Name = name;
        Notes = notes;
    }

    public void AddExercise(Exercise exercise)
    {
        _exercises.Add(exercise);
    }
}

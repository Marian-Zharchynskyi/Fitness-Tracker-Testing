namespace Domain.Workouts;

public class Exercise
{
    public ExerciseId Id { get; }
    public WorkoutId WorkoutId { get; private set; }
    public string Name { get; private set; }
    
    public int? Sets { get; private set; }
    public int? Reps { get; private set; }
    public decimal? WeightKg { get; private set; }
    public int? DurationSeconds { get; private set; }

    private Exercise(ExerciseId id, WorkoutId workoutId, string name)
    {
        Id = id;
        WorkoutId = workoutId;
        Name = name;
    }

    public static Exercise New(ExerciseId id, WorkoutId workoutId, string name)
        => new(id, workoutId, name);

    public void SetMetrics(int sets, int reps, decimal? weightKg = null, int? durationSeconds = null)
    {
        if (sets <= 0)
        {
            throw new ArgumentException("Sets must be a positive integer", nameof(sets));
        }

        if (reps <= 0)
        {
            throw new ArgumentException("Reps must be a positive integer", nameof(reps));
        }

        if (weightKg is < 0)
        {
            throw new ArgumentException("Weight must be non-negative", nameof(weightKg));
        }

        if (durationSeconds is < 0)
        {
            throw new ArgumentException("Duration must be non-negative", nameof(durationSeconds));
        }

        Sets = sets;
        Reps = reps;
        WeightKg = weightKg;
        DurationSeconds = durationSeconds;
    }
}

using Domain.Users;
using Domain.Workouts;

namespace Tests.Data;

public static class WorkoutsData
{
    public static Workout CreateWorkout(UserId userId, string name, DateTime date, int duration, int calories)
    {
        var workout = Workout.New(new WorkoutId(Guid.NewGuid()), userId, name);
        workout.SetDate(date);
        workout.SetMetrics(duration, calories);
        return workout;
    }

    public static Exercise CreateExercise(WorkoutId workoutId, string name, int sets, int reps, decimal? weight = null, int? duration = null)
    {
        var exercise = Exercise.New(new ExerciseId(Guid.NewGuid()), workoutId, name);
        exercise.SetMetrics(sets, reps, weight, duration);
        return exercise;
    }
}

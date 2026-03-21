namespace Application.Workouts.Exceptions;

public class WorkoutException : Exception
{
    public WorkoutException(string message) : base(message) { }
}

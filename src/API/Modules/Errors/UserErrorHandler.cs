using Application.Users.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Modules.Errors;

public static class UserErrorHandler
{
    public static ObjectResult ToObjectResult(this UserException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                UserByThisEmailAlreadyExistsException =>
                    StatusCodes.Status409Conflict,

                UserNotFoundException =>
                    StatusCodes.Status404NotFound,

                EmailOrPasswordAreIncorrect => StatusCodes.Status401Unauthorized,

                UserUnknownException or ImageSaveException => StatusCodes.Status500InternalServerError,

                _ => throw new NotImplementedException("User error handler not implemented for this exception type")
            }
        };
    }
}
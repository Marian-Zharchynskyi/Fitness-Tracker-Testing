using Domain.Users;

namespace Application.Users.Exceptions;

public abstract class UserException(UserId id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public UserId UserId { get; } = id;
}

public class UserByThisEmailAlreadyExistsException(UserId id)
    : UserException(id, $"User by this email already exists! User id: {id}");

public class EmailOrPasswordAreIncorrect() : UserException(UserId.Empty, "Email or Password are incorrect!");

public class UserNotFoundException(UserId id) : UserException(id, $"User under id: {id} was not found!");

public class ImageSaveException(UserId id) : UserException(id, $"User under id: {id} have problems with image save!");

public class UserUnknownException(UserId id, Exception innerException)
    : UserException(id, $"Unknown exception for the user under id: {id}", innerException);

public class InvalidPasswordException(UserId id)
    : UserException(id, "Current password is incorrect!");

public class OAuthEmailCannotBeChangedException(UserId id)
    : UserException(id, "Email is linked to an OAuth provider and cannot be changed through this application.");
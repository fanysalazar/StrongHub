namespace StrongHub.Models;

/// <summary>Thrown when user input breaks a business rule.</summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

/// <summary>Thrown when a username does not exist.</summary>
public class UserNotFoundException : Exception
{
    public UserNotFoundException(string username)
        : base($"User '{username}' was not found.") { }
}

/// <summary>Thrown when a data file cannot be read or written.</summary>
public class DataAccessException : Exception
{
    public DataAccessException(string message) : base(message) { }
    public DataAccessException(string message, Exception inner) : base(message, inner) { }
}

namespace Common.Exceptions;

public class ResourceExistsException : Exception
{
    public ResourceExistsException()
    {
    }

    public ResourceExistsException(string message) : base(message)
    {
    }
}
namespace Common.Exceptions;

/// <summary>
/// Custom ResourceNotFoundException so code is not coupled to AWS Exceptions
/// </summary>
public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException()
    {
    }

    public ResourceNotFoundException(string message) : base(message) {}
}
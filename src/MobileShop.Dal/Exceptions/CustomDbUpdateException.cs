namespace MobileShop.Dal.Exceptions;

public class CustomDbUpdateException : CustomException
{
    public CustomDbUpdateException() { }

    public CustomDbUpdateException(string message)
        : base(message) { }

    public CustomDbUpdateException(string message, Exception innerException)
        : base(message, innerException) { }
}

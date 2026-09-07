namespace MobileShop.Dal.Exceptions;

public class CustomConcurrencyException : CustomException
{
    public CustomConcurrencyException() { }

    public CustomConcurrencyException(string message)
        : base(message) { }

    public CustomConcurrencyException(string message, Exception innerException)
        : base(message, innerException) { }
}

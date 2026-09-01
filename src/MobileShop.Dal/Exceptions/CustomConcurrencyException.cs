namespace MobileShop.Dal.Exceptions;

public class CustomConcurrencyException(string message, Exception? innerException = null)
    : CustomException(message, innerException);

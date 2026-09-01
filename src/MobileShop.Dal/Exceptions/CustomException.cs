namespace MobileShop.Dal.Exceptions;

public class CustomException(string message, Exception? innerException = null)
    : Exception(message, innerException);

namespace MobileShop.Dal.Exceptions;

public class CustomDbUpdateException(string message, Exception? innerException = null)
    : CustomException(message, innerException);

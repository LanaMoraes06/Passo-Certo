namespace PassoCerto.Web.Exceptions;

public class ForbiddenException(string message) : Exception(message)
{
}
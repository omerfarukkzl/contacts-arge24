namespace Contacts.Api.Exceptions;

public sealed class DuplicatePhoneException(string message) : Exception(message);

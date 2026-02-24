namespace WholesaleHub.Application.Common;

// -> 404
public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} '{key}' was not found.");

// -> 409. duplicates, overlapping agreements, someone else saved first, etc
public sealed class ConflictException(string message) : Exception(message);

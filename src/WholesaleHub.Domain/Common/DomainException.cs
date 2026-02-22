namespace WholesaleHub.Domain.Common;

// thrown when something breaks a business rule -> api turns it into a 422
public class DomainException(string message) : Exception(message);

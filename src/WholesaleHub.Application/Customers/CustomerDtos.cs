using WholesaleHub.Domain.Customers;

namespace WholesaleHub.Application.Customers;

public sealed record CustomerRequest(
    string LegalName,
    string BusinessNumber,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    string ShippingAddress,
    PaymentTerms PaymentTerms);

public sealed record CustomerDto(
    Guid Id,
    string LegalName,
    string BusinessNumber,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    string ShippingAddress,
    PaymentTerms PaymentTerms,
    DateTimeOffset CreatedAt);

internal static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer c) =>
        new(c.Id, c.LegalName, c.BusinessNumber, c.ContactName, c.ContactEmail, c.ContactPhone,
            c.ShippingAddress, c.PaymentTerms, c.CreatedAt);
}

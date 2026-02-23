using System.Text.RegularExpressions;
using WholesaleHub.Domain.Common;

namespace WholesaleHub.Domain.Customers;

// A business that buys from us (shop, restaurant, contractor...)
public sealed partial class Customer : Entity
{
    public string LegalName { get; private set; } = null!;
    public string BusinessNumber { get; private set; } = null!; // CRA business number, 9 digits
    public string ContactName { get; private set; } = null!;
    public string ContactEmail { get; private set; } = null!;
    public string? ContactPhone { get; private set; }
    public string ShippingAddress { get; private set; } = null!;
    public PaymentTerms PaymentTerms { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }

    private Customer() { } // for EF

    public static Customer Create(
        string legalName,
        string businessNumber,
        string contactName,
        string contactEmail,
        string? contactPhone,
        string shippingAddress,
        PaymentTerms paymentTerms,
        DateTimeOffset now)
    {
        var customer = new Customer { CreatedAt = now };
        customer.Update(legalName, businessNumber, contactName, contactEmail, contactPhone, shippingAddress, paymentTerms);
        return customer;
    }

    public void Update(
        string legalName,
        string businessNumber,
        string contactName,
        string contactEmail,
        string? contactPhone,
        string shippingAddress,
        PaymentTerms paymentTerms)
    {
        LegalName = Guard.NotBlank(legalName, "Legal name", 200);
        ContactName = Guard.NotBlank(contactName, "Contact name", 120);
        // lowercase so we don't end up with Bob@x.com and bob@x.com as "different" people
        ContactEmail = Guard.NotBlank(contactEmail, "Contact email", 254).ToLowerInvariant();
        ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim();
        ShippingAddress = Guard.NotBlank(shippingAddress, "Shipping address", 300);

        if (!Enum.IsDefined(paymentTerms))
            throw new DomainException("Unknown payment terms.");
        PaymentTerms = paymentTerms;

        // TODO: people paste BNs with spaces ("123 456 789") - strip those instead of rejecting?
        var bn = Guard.NotBlank(businessNumber, "Business number", 9);
        if (!NineDigits().IsMatch(bn))
            throw new DomainException("Business number must be exactly 9 digits.");
        BusinessNumber = bn;
    }

    [GeneratedRegex(@"^\d{9}$")]
    private static partial Regex NineDigits();
}

namespace WholesaleHub.Domain.Agreements;

public enum AgreementStatus
{
    Draft,     // quoted, customer hasn't signed yet
    Active,
    Cancelled,
    Expired,   // never saved to the db, worked out from EndDate (see StatusOn)
}

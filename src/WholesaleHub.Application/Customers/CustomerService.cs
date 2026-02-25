using FluentValidation;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Application.Common;
using WholesaleHub.Domain.Customers;

namespace WholesaleHub.Application.Customers;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> SearchAsync(string? search, int? page, int? pageSize, CancellationToken ct);
    Task<CustomerDto> GetAsync(Guid id, CancellationToken ct);
    Task<CustomerDto> CreateAsync(CustomerRequest request, CancellationToken ct);
    Task<CustomerDto> UpdateAsync(Guid id, CustomerRequest request, CancellationToken ct);
}

public sealed class CustomerService(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    IValidator<CustomerRequest> validator,
    TimeProvider clock) : ICustomerService
{
    public async Task<PagedResult<CustomerDto>> SearchAsync(string? search, int? page, int? pageSize, CancellationToken ct)
    {
        var (p, size) = Paging.Normalize(page, pageSize);
        var result = await customers.SearchAsync(search?.Trim(), p, size, ct);
        return new PagedResult<CustomerDto>([.. result.Items.Select(c => c.ToDto())], result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<CustomerDto> GetAsync(Guid id, CancellationToken ct) => (await LoadAsync(id, ct)).ToDto();

    public async Task<CustomerDto> CreateAsync(CustomerRequest request, CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);

        if (await customers.BusinessNumberExistsAsync(request.BusinessNumber.Trim(), null, ct))
            throw new ConflictException($"A customer with business number {request.BusinessNumber} already exists.");

        var customer = Customer.Create(request.LegalName, request.BusinessNumber, request.ContactName,
            request.ContactEmail, request.ContactPhone, request.ShippingAddress, request.PaymentTerms, clock.GetUtcNow());

        customers.Add(customer);
        await unitOfWork.SaveChangesAsync(ct);
        return customer.ToDto();
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, CustomerRequest request, CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);
        var customer = await LoadAsync(id, ct);

        if (await customers.BusinessNumberExistsAsync(request.BusinessNumber.Trim(), id, ct))
            throw new ConflictException($"A customer with business number {request.BusinessNumber} already exists.");

        // TODO: changing payment terms should probably be logged somewhere (finance will ask)
        customer.Update(request.LegalName, request.BusinessNumber, request.ContactName, request.ContactEmail,
            request.ContactPhone, request.ShippingAddress, request.PaymentTerms);

        await unitOfWork.SaveChangesAsync(ct);
        return customer.ToDto();
    }

    private async Task<Customer> LoadAsync(Guid id, CancellationToken ct) =>
        await customers.GetByIdAsync(id, ct) ?? throw new NotFoundException("Customer", id);
}

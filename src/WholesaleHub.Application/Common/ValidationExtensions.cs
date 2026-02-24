using FluentValidation;

namespace WholesaleHub.Application.Common;

internal static class ValidationExtensions
{
    public static async Task EnsureValidAsync<T>(this IValidator<T> validator, T instance, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
    }
}

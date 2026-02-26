using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Application.Common;

namespace WholesaleHub.Infrastructure.Persistence;

internal sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Someone else changed this record while you were editing. Reload and try again.");
        }
        // 2601/2627 = unique index / unique constraint violated.
        // The services already check for duplicates, this catches the race where two requests insert at once.
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException("A record with the same unique value already exists.");
        }
    }
}

using StackFlow.Application.Common.Interfaces;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.Infrastructure.Persistence;

// Unit of Work — wraps AppDbContext.SaveChangesAsync behind the IUnitOfWork interface.
//
// Handlers call this at the end of every mutation to commit all pending changes
// in a single database round-trip. All repository operations in one handler
// share the same DbContext instance (scoped lifetime) and are therefore part
// of the same implicit EF Core transaction.
//
// Why wrap it at all: handlers depend on IUnitOfWork (Application interface),
// not on AppDbContext (Infrastructure). This keeps handlers free of any
// Infrastructure dependency.
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}

namespace StackFlow.Application.Common.Interfaces;

// Unit of Work — the single point of transaction control for all handlers.
//
// Handlers always call SaveChangesAsync at the end of a mutation, never repositories.
// This ensures that multiple repository operations within one handler are committed
// atomically in a single database round-trip.
//
// Example:
//   await _repo.AddAsync(entity, ct);
//   await _auditRepo.AddAsync(audit, ct);
//   await _uow.SaveChangesAsync(ct);  // both writes committed together
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}

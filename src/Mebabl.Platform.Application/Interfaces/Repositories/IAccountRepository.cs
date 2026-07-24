using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Application.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);

    Task<Account?> GetByNameAsync(string name);

    Task AddAsync(Account account);

    Task UpdateAsync(Account account);

    Task DeleteAsync(Account account);
}
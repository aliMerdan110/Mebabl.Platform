using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);

    Task<Role?> GetByNameAsync(string name);

    Task<List<Role>> GetAllAsync();

    Task AddAsync(Role role);

    Task UpdateAsync(Role role);

    Task DeleteAsync(Role role);
}
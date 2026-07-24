using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Application.Interfaces.Repositories;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id);

    Task<Permission?> GetByNameAsync(string name);

    Task<List<Permission>> GetAllAsync();
}
using Mebabl.Platform.Domain.Entities;

namespace Mebabl.Platform.Application.Interfaces.Repositories;

public interface IApplicationUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid id);

    Task<ApplicationUser?> GetByEmailAsync(string email);

    Task<ApplicationUser?> GetByUsernameAsync(string username);

    Task AddAsync(ApplicationUser user);

    Task UpdateAsync(ApplicationUser user);

    Task DeleteAsync(ApplicationUser user);
}
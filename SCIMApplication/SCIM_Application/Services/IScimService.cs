using SCIM_Application.Models;

namespace SCIM_Application.Services
{
    public interface IScimService
    {
        Task<bool> CreateUserAsync(User user, Application application, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(User user, Application application, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(User user, Application application, CancellationToken cancellationToken = default);
    }
}

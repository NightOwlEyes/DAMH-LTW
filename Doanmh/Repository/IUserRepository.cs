using Doanmh.Model;

namespace Doanmh.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetUserAsync();
        Task<User> GetUserByIdAsync(int id);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> RegisterAsync(RegisterDto dto);
        Task<bool> RegisterAdminAsync(RegisterDto dto);
        Task<User> LoginAsync(string username, string password);
    }
}

using respNewsV8.Models;

namespace respNewsV8.Services
{
    public interface IUserService
    {
        bool IsValidUser(User user);
    }

    public class UserService : IUserService
    {
        private readonly RespNewContext _context;

        public UserService(RespNewContext context)
        {
            _context = context;
        }

        public bool IsValidUser(User user)
        {
            try
            {
                var foundUser = _context.Users
                    .FirstOrDefault(u => u.UserName == user.UserName && u.UserPassword == user.UserPassword);

                if (foundUser == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}

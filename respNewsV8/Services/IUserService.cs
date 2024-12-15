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
                    .FirstOrDefault(u => u.UserName == user.UserName);

                // Kullanıcı bulunamadıysa veya şifre yanlışsa
                if (foundUser == null || foundUser.UserPassword != user.UserPassword)
                {
                    return false;
                }

                // Kullanıcı rolünü kontrol et
                if (foundUser.UserRole != "Admin")  // Burada "Admin" rolünü kontrol ediyorsunuz
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

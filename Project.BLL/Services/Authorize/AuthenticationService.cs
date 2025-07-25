using Microsoft.AspNet.Identity.Owin;

namespace Project.BLL.Services.Authorize
{
    public class AuthenticationService
    {

        public static SignInStatus SingInLocal(string password, string storedHash, string storedSalt)
        {

            if (PasswordService.VerifyPasswordHash(password, storedHash, storedSalt))
            {
                return SignInStatus.Success;
            }

            return SignInStatus.Failure;
        }

    }
}

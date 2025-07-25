using System.Security.Cryptography;
using System.Text;


namespace Project.BLL.Services.Authorize
{
    public class PasswordService
    {

        public static string RadomPassword(int length = 16)
        {

            const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-";
            Random random = new Random();

            StringBuilder passwordBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                passwordBuilder.Append(allowedChars[random.Next(0, allowedChars.Length)]);
            }
            return passwordBuilder.ToString();
        }


        public static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {

            byte[] saltBytes = new byte[32];
            using (var randomGenerator = RandomNumberGenerator.Create())
            {
                randomGenerator.GetBytes(saltBytes);
            }
            passwordSalt = Convert.ToBase64String(saltBytes);
            byte[] salt = Encoding.UTF8.GetBytes(passwordSalt);

            using var hmacSha512 = new HMACSHA512(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = hmacSha512.ComputeHash(passwordBytes);
            passwordHash = Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
        {

            byte[] salt = Encoding.UTF8.GetBytes(passwordSalt);
            using var hmacSha512 = new HMACSHA512(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = hmacSha512.ComputeHash(passwordBytes);
            string hashedEnteredPassword = Convert.ToBase64String(hashBytes);

            return hashedEnteredPassword == passwordHash;
        }
    }
}
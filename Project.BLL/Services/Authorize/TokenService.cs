using Microsoft.IdentityModel.Tokens;
using Project.DTO.DB;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project.BLL.Services.Authorize
{
    public static class TokenService
    {
        public static string GenerateToken(TB_ADM_USUARIO user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();


            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Id", user.ID.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.USUARIO),
                    new Claim(ClaimTypes.Name, user.NOME),
                    new Claim(ClaimTypes.Email, user.EMAIL),
                    new Claim(ClaimTypes.Role, "PERFIL".ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Settings.SecretToken)), SecurityAlgorithms.HmacSha256Signature),

            });

            return tokenHandler.WriteToken(token);
        }



        public static Guid ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Settings.SecretToken);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero

                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "Id").Value);
                return accountId;
            }
            catch
            {
                return Guid.Empty;
            }
        }
    }

    public static class Settings
    {
        public static string SecretToken = "SGMACsTSGwJczMYRP3uBLz5E6BTSx9EKj";
    }
}

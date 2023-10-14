using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyRazorPage.Utils
{
    public static class JwtUtils
    {

        public static String GenAccessToken(
            IEnumerable<Claim> claims,
            string uniqueKey,
            double exp)
        {
            return new JwtSecurityTokenHandler().WriteToken(GetJwtToken(claims, uniqueKey, exp));
        }
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public static ClaimsPrincipal GetPrincipalFrom(string token, string uniqueKey)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(uniqueKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private static JwtSecurityToken GetJwtToken(
                IEnumerable<Claim> claims,
                string uniqueKey,
                double exp)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(uniqueKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: "https://localhost:5070",
                audience: "https://localhost:5070",
                expires: DateTime.UtcNow.AddMinutes(exp),
                claims: claims,
                signingCredentials: creds);
        }

        public static List<Claim> GetClaimsOf(Models.Account account, DateTime expiration)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.Name, account.AccountId.ToString()),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role.ToString()),
                new Claim(ClaimTypes.Expiration, expiration.ToString())
            };
        }

         public static bool isTokenValid(ClaimsPrincipal principal)
        {
            DateTime.TryParse(principal.FindFirst(c =>c.Type == ClaimTypes.Expiration)?.Value, out DateTime exp);
            return exp > DateTime.Now;
        }


    }
}

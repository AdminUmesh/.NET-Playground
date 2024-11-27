using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Azure.Core;


namespace DotNet_Playground.Controllers
{
    public class JWTController : Controller
    {
        public IActionResult JWTlogin()
        {
            return View();
        }

        public IActionResult Loginrequest(string id, string pass)
        {
            string comp_code = "001";
            string Gen_Token = comp_code + "#" + id + "#";
            string Token = GlobalTokenFunction.GenerateToken(Gen_Token + "CsatSpl37");


            //change by Yourself
            string username = "a";
            string password = "a";
            string Pass_exp = "11/12/2000";
            string CompanyName = "umesh";
            HttpContext.Session.SetString("Token", Token);
            //FormsAuthentication.SetAuthCookie(id + "~" + username + "~" + password + "~" + Pass_exp + "~" + CompanyName, true);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, id),
                new Claim("Username", username),
                new Claim("Password", password),
                new Claim("PassExp", Pass_exp),
                new Claim("CompanyName", CompanyName),
                new Claim("Token", Token) // Optional, depending on what you want to store
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in the user by setting the authentication cookie
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // Set the JWT token as a cookie for later validation
            Response.Cookies.Append("JWT", Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1) // Token expiration
            });

            return new EmptyResult();
        }

        public IActionResult validateTocken(string token)
        {
            // Retrieve the JWT token from the cookie (or session if needed)
            //string token = Request.Cookies["JWT"];

            if (string.IsNullOrEmpty(token))
            {
                // If no token is found in cookies, return an error or redirect
                return Unauthorized("Token is missing or invalid.");
            }

            // Validate the token using the GlobalTokenFunction
            string username = GlobalTokenFunction.ValidateToken(token);

            if (username == null)
            {
                // Token validation failed, return an error
                return Unauthorized("Invalid token.");
            }

            // If valid, you can perform other actions or return user details
            return Ok(new { Message = "Token is valid", Username = username });
        }
    }

    public class GlobalTokenFunction
    {
        private static string Secret = "ERMN05OPLoDvbTTa/QkqLNMI7cPLguaRyHzyg7n5qNBVjQmtBhz4SzYh4NBVCXi3KJHlSXKP+oi2+bXr6CUYTR==";

        public static string GenerateToken(string username)
        {
            byte[] key = Convert.FromBase64String(Secret);
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(key);
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, username)
            }),
                Expires = DateTime.UtcNow.AddMinutes(36000),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }

        public static string ValidateToken(string token)
        {
            string username = null;
            ClaimsPrincipal principal = GetPrincipal(token);
            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (NullReferenceException)
            {
                return null;
            }
            Claim usernameClaim = identity.FindFirst(ClaimTypes.Name);
            username = usernameClaim.Value;
            return username;
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if (jwtToken == null) return null;
                byte[] key = Convert.FromBase64String(Secret);
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }

    
}

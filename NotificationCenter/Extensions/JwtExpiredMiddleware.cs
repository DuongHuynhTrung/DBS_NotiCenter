using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net;

namespace NotificationCenter.Extensions
{
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            var isStaticResourceRequest = context.Request.Path.StartsWithSegments("/message");
            if (isStaticResourceRequest)
            {
                if (!await AuthenticateRequest(context))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
            }
            await _next(context);

        }

        private async Task<bool> AuthenticateRequest(HttpContext context)
        {
            var tokenString = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            bool isValidToken = await ValidateJwtToken(tokenString);
            return isValidToken;
        }

        private async Task<bool> ValidateJwtToken(string tokenString)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(tokenString, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateAudience = false,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    RequireAudience = false,
                }, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }


}


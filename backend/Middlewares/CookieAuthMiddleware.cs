using GaraShowcase.Api.Utils;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GaraShowcase.Api.Middlewares
{
    public class CookieAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue("token", out var token))
            {
                var principal = TokenHelper.VerifyToken(token);
                if (principal != null)
                {
                    context.User = principal;
                }
            }

            await _next(context);
        }
    }
}

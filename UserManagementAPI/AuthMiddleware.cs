using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string TOKEN_VALIDO = "Bearer mi_token_secreto"; // Cambia esto por tu lógica real

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || authHeader != TOKEN_VALIDO)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"No autorizado\"}");
            return;
        }

        await _next(context);
    }
}
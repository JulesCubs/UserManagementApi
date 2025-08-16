using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class AuditoriaMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditoriaMiddleware> _logger;

    public AuditoriaMiddleware(RequestDelegate next, ILogger<AuditoriaMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var metodo = context.Request.Method;
        var ruta = context.Request.Path;

        await _next(context);

        var codigoEstado = context.Response.StatusCode;

        _logger.LogInformation("Auditoría: Método={Metodo}, Ruta={Ruta}, Estado={CodigoEstado}",
            metodo, ruta, codigoEstado);
    }
}
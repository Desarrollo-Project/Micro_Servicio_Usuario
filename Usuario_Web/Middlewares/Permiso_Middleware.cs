using System.Security.Claims;

public class Permiso_Middleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<Permiso_Middleware> _logger;

    public Permiso_Middleware(RequestDelegate next, ILogger<Permiso_Middleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var metadata = endpoint.Metadata.GetMetadata<Permiso_Requerido>();
        if (metadata == null)
        {
            await _next(context);
            return;
        }

        var user = context.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("No autenticado.");
            return;
        }

        var permisos = user.Claims
            .Where(c => c.Type == "permisos")
            .Select(c => c.Value)
            .ToList();

        if (!permisos.Contains(metadata.Permiso))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync($"No tienes el permiso requerido: {metadata.Permiso}");
            return;
        }

        await _next(context);
    }
}
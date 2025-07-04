using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Usuario_Domain.Interfaces;

public class Notificaciones_Cliente : INotificaciones_Cliente
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Notificaciones_Cliente> _logger;

    public Notificaciones_Cliente(HttpClient httpClient, ILogger<Notificaciones_Cliente> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task EnviarNotificacionCambioClaveAsync(string email, string nombre)
    {
        var payload = new { email, nombre };
        await PostAsync("/api/notificaciones/cambio-clave", payload);
    }

    public async Task EnviarCorreoConfirmacionAsync(string email, string nombre, string codigo)
    {
        var payload = new { email, nombre, codigo };
        await PostAsync("/api/notificaciones/correo-confirmacion", payload);
    }

    public async Task EnviarTokenRecuperacionAsync(string email, string nombre, string token)
    {
        var payload = new { email, nombre, token };
        await PostAsync("/api/notificaciones/token-recuperacion", payload);
    }

    private async Task PostAsync(string endpoint, object payload)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, payload);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error al llamar a {Endpoint}: {StatusCode} - {Reason}", endpoint, response.StatusCode, response.ReasonPhrase);
                throw new ApplicationException($"Error al llamar a {endpoint}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al llamar a {Endpoint}", endpoint);
            throw;
        }
    }
}
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Usuario_Domain.Entities;
using Usuario_Domain.Interfaces;


namespace Usuario_Application.Services;


public class Keycloak_Servicio : IKeycloak_Servicio
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public Keycloak_Servicio(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    private async Task<string> ObtenerTokenAdmin()
    {
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _configuration["Keycloak:ClientId"] },
            { "client_secret", _configuration["Keycloak:ClientSecret"] },
            { "grant_type", "client_credentials" }
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.PostAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/realms/{_configuration["Keycloak:realm"]}/protocol/openid-connect/token",
            content
        );

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<string> Crear_Usuario_Keycloak(Usuario usuario)
    {
        var token = await ObtenerTokenAdmin();

        var payload = new
        {
            username = usuario.Correo,
            email = usuario.Correo,
            firstName = usuario.Nombre,
            lastName = usuario.Apellido,
            enabled = true,
            credentials = new[]
            {
                new {
                    type = "password",
                    value = usuario.Password,
                    temporary = false
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.PostAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/users",
            content
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al crear usuario en Keycloak: {error}");
        }

        // Obtener Location header que incluye el ID creado
        var location = response.Headers.Location?.ToString();
        return location?.Split('/').Last() ?? throw new Exception("No se pudo obtener el ID del usuario creado");
    }

    public async Task Asignar_Rol_Usuario_Keycloak(string keycloak_Id, string rol)
    {
        var token = await ObtenerTokenAdmin();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Obtener todos los roles actuales del usuario
        var currentRolesResponse = await _httpClient.GetAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/users/{keycloak_Id}/role-mappings/realm");

        currentRolesResponse.EnsureSuccessStatusCode();
        var currentRolesJson = await currentRolesResponse.Content.ReadAsStringAsync();
        var currentRoles = JsonSerializer.Deserialize<List<KeycloakRole>>(currentRolesJson)!;

        // Eliminar todos los roles actuales
        var rolesToRemove = currentRoles
            .Where(r => !r.name.StartsWith("default-roles-"))
            .ToList(); // EL Rol Default no

        if (rolesToRemove.Any())
        {
            var deleteContent = new StringContent(JsonSerializer.Serialize(rolesToRemove), Encoding.UTF8, "application/json");
            var deleteResponse = await _httpClient.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/users/{keycloak_Id}/role-mappings/realm"),
                Content = deleteContent
            });
            deleteResponse.EnsureSuccessStatusCode();
        }

        // Obtener el nuevo rol a asignar
        var allRolesResponse = await _httpClient.GetAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles");
        allRolesResponse.EnsureSuccessStatusCode();

        var allRolesJson = await allRolesResponse.Content.ReadAsStringAsync();
        var allRoles = JsonSerializer.Deserialize<List<KeycloakRole>>(allRolesJson)!;

        var nuevoRol = allRoles.FirstOrDefault(r => r.name == rol)
            ?? throw new Exception($"Rol '{rol}' no encontrado en Keycloak");

        // Asignar solo el nuevo rol
        var assignContent = new StringContent(JsonSerializer.Serialize(new[] { nuevoRol }), Encoding.UTF8, "application/json");
        var assignResponse = await _httpClient.PostAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/users/{keycloak_Id}/role-mappings/realm",
            assignContent);
        assignResponse.EnsureSuccessStatusCode();
    }

    public async Task Actualizar_Usuario_Keycloak(Usuario usuario)
    {
        var token = await ObtenerTokenAdmin();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Actualizar datos generales (nombre, correo, etc.)
        var payload = new
        {
            firstName = usuario.Nombre,
            lastName = usuario.Apellido,
            email = usuario.Correo,
            enabled = true
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var updateResponse = await _httpClient.PutAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/users/{usuario.KeycloakId}",
            content
        );

        updateResponse.EnsureSuccessStatusCode();

        // 2. Cambiar contraseña
        var passwordPayload = new
        {
            type = "password",
            value = usuario.Password,
            temporary = false
        };

        var passwordContent = new StringContent(JsonSerializer.Serialize(passwordPayload), Encoding.UTF8, "application/json");

        var passwordResponse = await _httpClient.PutAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/users/{usuario.KeycloakId}/reset-password",
            passwordContent
        );

        passwordResponse.EnsureSuccessStatusCode();
    }


    public async Task<List<RolConPermisos>> ObtenerRolesCompuestos()
    {
        var token = await ObtenerTokenAdmin();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var rolesResponse = await _httpClient.GetAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles");
        rolesResponse.EnsureSuccessStatusCode();

        var rolesJson = await rolesResponse.Content.ReadAsStringAsync();
        var roles = JsonSerializer.Deserialize<List<KeycloakRole>>(rolesJson)!;

        var compuestos = new List<RolConPermisos>();

        foreach (var rol in roles)
        {
            if (rol.name.StartsWith("default-roles-"))
                continue;

            var compositesResponse = await _httpClient.GetAsync(
                $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles/{rol.name}/composites");

            if (!compositesResponse.IsSuccessStatusCode)
                continue;

            var compositesJson = await compositesResponse.Content.ReadAsStringAsync();
            var hijos = JsonSerializer.Deserialize<List<KeycloakRole>>(compositesJson)!;

            if (hijos.Any())
            {
                compuestos.Add(new RolConPermisos
                {
                    name = rol.name,
                    compositeRoles = hijos
                });
            }
        }

        return compuestos;
    }


    public async Task Modificar_Permisos_Rol(string rolPrincipal, List<string> nuevosPermisos)
    {
        var token = await ObtenerTokenAdmin();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Obtener los roles hijos actuales
        var actualesRes = await _httpClient.GetAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles/{rolPrincipal}/composites");

        actualesRes.EnsureSuccessStatusCode();
        var actualesJson = await actualesRes.Content.ReadAsStringAsync();
        var actuales = JsonSerializer.Deserialize<List<KeycloakRole>>(actualesJson)!;

        // Obtener todos los roles posibles
        var allRolesRes = await _httpClient.GetAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles");

        allRolesRes.EnsureSuccessStatusCode();
        var allRolesJson = await allRolesRes.Content.ReadAsStringAsync();
        var allRoles = JsonSerializer.Deserialize<List<KeycloakRole>>(allRolesJson)!;

        // Eliminar todos los roles actuales que no sean "default-roles-*"
        var rolesAEliminar = actuales.Where(r => !r.name.StartsWith("default-roles-")).ToList();

        if (rolesAEliminar.Any())
        {
            var eliminarContent = new StringContent(JsonSerializer.Serialize(rolesAEliminar), Encoding.UTF8, "application/json");
            var eliminarRes = await _httpClient.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles/{rolPrincipal}/composites"),
                Content = eliminarContent
            });
            eliminarRes.EnsureSuccessStatusCode();
        }

        // Asignar los nuevos permisos
        var permisosAsignar = allRoles
            .Where(r => nuevosPermisos.Contains(r.name))
            .ToList();

        if (permisosAsignar.Any())
        {
            var asignarContent = new StringContent(JsonSerializer.Serialize(permisosAsignar), Encoding.UTF8, "application/json");
            var asignarRes = await _httpClient.PostAsync(
                $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles/{rolPrincipal}/composites",
                asignarContent);

            asignarRes.EnsureSuccessStatusCode();
        }
    }

    public async Task<List<KeycloakRole>> ObtenerRolesSimples()
    {
        var token = await ObtenerTokenAdmin();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Obtener todos los roles
        var allRolesResponse = await _httpClient.GetAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles");

        allRolesResponse.EnsureSuccessStatusCode();

        var allRolesJson = await allRolesResponse.Content.ReadAsStringAsync();
        var allRoles = JsonSerializer.Deserialize<List<KeycloakRole>>(allRolesJson)!;

        var simples = new List<KeycloakRole>();

        foreach (var rol in allRoles)
        {
            if (rol.name.StartsWith("default-roles-"))
                continue;

            var compositesResponse = await _httpClient.GetAsync(
                $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles/{rol.name}/composites");

            if (!compositesResponse.IsSuccessStatusCode)
                continue;

            var compositesJson = await compositesResponse.Content.ReadAsStringAsync();
            var subRoles = JsonSerializer.Deserialize<List<KeycloakRole>>(compositesJson)!;

            if (!subRoles.Any())
                simples.Add(rol);
        }

        return simples;
    }


    public async Task<bool> AgregarSubrolAsync(string rolPrincipal, string subrol)
    {
        var token = await ObtenerTokenAdmin();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var allRoles = await ObtenerRolesSimples();
        var rol = allRoles.FirstOrDefault(r => r.name == subrol);
        if (rol is null) return false;

        var content = new StringContent(JsonSerializer.Serialize(new[] { rol }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(
            $"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles/{rolPrincipal}/composites", content);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EliminarSubrolAsync(string rolPrincipal, string subrol)
    {
        var token = await ObtenerTokenAdmin();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var allRoles = await ObtenerRolesSimples();
        var rol = allRoles.FirstOrDefault(r => r.name == subrol);
        if (rol is null) return false;

        var content = new StringContent(JsonSerializer.Serialize(new[] { rol }), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{_configuration["Keycloak:auth-server-url"]}/admin/realms/{_configuration["Keycloak:realm"]}/roles/{rolPrincipal}/composites"),
            Content = content
        };

        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }


}
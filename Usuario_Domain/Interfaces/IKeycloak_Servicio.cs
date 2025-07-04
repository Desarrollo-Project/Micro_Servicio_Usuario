using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface IKeycloak_Servicio
{
    Task<string> Crear_Usuario_Keycloak(Usuario usuario);
    Task Asignar_Rol_Usuario_Keycloak(string keycloak_Id, string rol);
    Task Actualizar_Usuario_Keycloak(Usuario usuario);

    Task<List<RolConPermisos>> ObtenerRolesCompuestos();
    Task<List<KeycloakRole>> ObtenerRolesSimples();
    Task Modificar_Permisos_Rol(string rolPrincipal, List<string> nuevosPermisos);
    Task<bool> AgregarSubrolAsync(string rolPrincipal, string subrol);
    Task<bool> EliminarSubrolAsync(string rolPrincipal, string subrol);
}


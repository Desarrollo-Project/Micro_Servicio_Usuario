namespace Usuario_Domain.Entities;

public class RolConPermisos
{
    public string name { get; set; } = default!;
    public List<KeycloakRole> compositeRoles { get; set; } = new();
}
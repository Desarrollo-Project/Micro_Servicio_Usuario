namespace Usuario_Application.DTOs;

public class Dto_Modificar_Permisos_Rol
{
    public string RolPrincipal { get; set; } = null!;
    public List<string> Permisos { get; set; } = new(); // Lista final deseada de permisos
}
namespace Usuario_Application.DTOs;

public class Dto_Rol_Con_Permisos
{
    public string Nombre { get; set; } = null!;
    public List<string> Permisos { get; set; } = new();
}
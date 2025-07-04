[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class Permiso_Requerido: Attribute
{
    public string Permiso { get; }

    public Permiso_Requerido(string permiso)
    {
        Permiso = permiso;
    }
}
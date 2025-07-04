using Microsoft.EntityFrameworkCore;
using Usuario_Domain.Entities;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;
using Usuario_Infrastructure.Persistance.DataBase;

namespace Usuario_Infrastructure.Persistance.Repositories;

/// <summary>
/// Repositorio encargado de acceder a la entidad Rol en la base de datos.
/// </summary>
public class Rol_Repositorio : IRol_Repositorio
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Inicializa el repositorio de roles con el contexto de base de datos.
    /// </summary>
    /// <param name="context">Instancia de AppDbContext.</param>
    public Rol_Repositorio(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene un rol por su identificador único.
    /// </summary>
    /// <param name="id">ID del rol a buscar.</param>
    /// <returns>Instancia de Rol si se encuentra; de lo contrario, null.</returns>
    /// <exception cref="Excepcion_Datos_Postgre">Si hay un fallo de acceso a datos.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Para errores inesperados.</exception>
    public async Task<Rol> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (DbUpdateException ex)
        {
            throw new Excepcion_Datos_Postgre("Error al consultar roles en la base de datos.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al obtener el rol.", ex);
        }
    }
}
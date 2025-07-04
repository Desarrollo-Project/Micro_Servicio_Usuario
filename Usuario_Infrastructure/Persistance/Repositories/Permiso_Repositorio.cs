using Microsoft.EntityFrameworkCore;
using Usuario_Domain.Entities;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;
using Usuario_Infrastructure.Persistance.DataBase;

namespace Usuario_Infrastructure.Persistance.Repositories;

/// <summary>
/// Repositorio para acceder a entidades de Permiso en la base de datos.
/// </summary>
public class Permiso_Repositorio : IPermiso_Repositorio
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio usando el contexto de la aplicación.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public Permiso_Repositorio(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene un permiso por su identificador único.
    /// </summary>
    /// <param name="id">ID del permiso.</param>
    /// <returns>Instancia de Permiso si se encuentra; de lo contrario, null.</returns>
    /// <exception cref="Excepcion_Datos_Postgre">Si ocurre un error en la consulta.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Si ocurre un error inesperado.</exception>
    public async Task<Permiso> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Permisos.FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (DbUpdateException ex)
        {
            throw new Excepcion_Datos_Postgre("Error al consultar permisos en la base de datos.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al obtener el permiso.", ex);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Usuario_Domain.Entities;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;
using Usuario_Infrastructure.Persistance.DataBase;

namespace Usuario_Infrastructure.Persistance.Repositories;

/// <summary>
/// Repositorio encargado de registrar actividades en la base de datos relacional.
/// </summary>
public class ActividadRepository : IActividad_Repository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio de actividades con el contexto de base de datos.
    /// </summary>
    /// <param name="context">Contexto de Entity Framework.</param>
    public ActividadRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registra una actividad en la tabla correspondiente.
    /// </summary>
    /// <param name="actividad">Entidad de actividad a persistir.</param>
    /// <returns>Tarea asincrónica.</returns>
    /// <exception cref="Excepcion_Datos_Postgre">Si ocurre un error al registrar la actividad.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Para errores no esperados.</exception>
    public async Task RegistrarActividad(Actividad actividad)
    {
        try
        {
            await _context.Actividades.AddAsync(actividad);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Excepcion_Datos_Postgre("Error al guardar la actividad en la base de datos relacional.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al registrar la actividad.", ex);
        }
    }
}
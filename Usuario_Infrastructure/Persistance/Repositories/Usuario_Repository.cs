using Microsoft.EntityFrameworkCore;
using Usuario_Domain.Entities;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;
using Usuario_Infrastructure.Persistance.DataBase;

namespace Usuario_Infrastructure.Persistance.Repositories;

/// <summary>
/// Repositorio responsable de gestionar las operaciones sobre la entidad Usuario en la base de datos relacional.
/// </summary>
public class Usuario_Repository : IUsuario_Repository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Inicializa el repositorio con el contexto de base de datos.
    /// </summary>
    /// <param name="context">Instancia de AppDbContext.</param>
    public Usuario_Repository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Agrega un nuevo usuario a la base de datos.
    /// </summary>
    /// <param name="usuario">Usuario a agregar.</param>
    /// <exception cref="Excepcion_Datos_Postgre">Si ocurre un error al guardar en base de datos.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Para errores inesperados.</exception>
    public async Task AddAsync(Usuario usuario)
    {
        try
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Excepcion_Datos_Postgre("Error al agregar el usuario a la base de datos.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al agregar el usuario.", ex);
        }
    }

    /// <summary>
    /// Actualiza un usuario existente en la base de datos.
    /// </summary>
    /// <param name="usuario">Usuario con los datos actualizados.</param>
    public async Task UpdateAsync(Usuario usuario)
    {
        try
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Excepcion_Datos_Postgre("Error al actualizar el usuario en la base de datos.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al actualizar el usuario.", ex);
        }
    }

    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    public async Task<Usuario> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error al obtener el usuario por ID.", ex);
        }
    }

    /// <summary>
    /// Busca un usuario por su dirección de correo electrónico.
    /// </summary>
    public async Task<Usuario> GetByEmail(string email)
    {
        try
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == email);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error al buscar el usuario por correo electrónico.", ex);
        }
    }

    /// <summary>
    /// Busca un usuario por su token de recuperación.
    /// </summary>
    public async Task<Usuario> GetByTokenRecuperacion(string token)
    {
        try
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.TokenRecuperacion == token);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error al buscar el usuario por token de recuperación.", ex);
        }
    }
}
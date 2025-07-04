using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usuario_Domain.Entities;

namespace Usuario_Infrastructure.Persistance.DataBase;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!context.Roles.Any() && !context.Permisos.Any() && !context.Rol_Permisos.Any())
        {
            var roles = new List<Rol>
            {
                new Rol { Id = 1, Nombre = "Administrador" },
                new Rol { Id = 2, Nombre = "Subastador" },
                new Rol { Id = 3, Nombre = "Postor" },
                new Rol { Id = 4, Nombre = "Soporte Tecnico" }
            };

            var permisos = new List<Permiso>
            {
                new Permiso { Id = 1, Descripcion = "gestionar usuarios" },
                new Permiso { Id = 2, Descripcion = "gestionar roles y permisos" },
                new Permiso { Id = 3, Descripcion = "administrar subastas" },
                new Permiso { Id = 4, Descripcion = "gestionar reportes" },
                new Permiso { Id = 5, Descripcion = "administrar medios de pago" },
                new Permiso { Id = 6, Descripcion = "gestionar reclamos y disputas" },
                new Permiso { Id = 7, Descripcion = "visualizar historial de transacciones" },
                new Permiso { Id = 8, Descripcion = "crear y administrar subastas" },
                new Permiso { Id = 9, Descripcion = "configurar productos" },
                new Permiso { Id = 10, Descripcion = "definir reglas de participación" },
                new Permiso { Id = 11, Descripcion = "validar pujas" },
                new Permiso { Id = 12, Descripcion = "notificar ganadores" },
                new Permiso { Id = 13, Descripcion = "revisar reclamos" },
                new Permiso { Id = 14, Descripcion = "explorar subastas" },
                new Permiso { Id = 15, Descripcion = "realizar pujas" },
                new Permiso { Id = 16, Descripcion = "pagar productos ganados" },
                new Permiso { Id = 17, Descripcion = "reclamar premios" },
                new Permiso { Id = 18, Descripcion = "presentar reclamos" },
                new Permiso { Id = 19, Descripcion = "visualizar historial de compras y pujas" },
                new Permiso { Id = 20, Descripcion = "resolver reclamos" },
                new Permiso { Id = 21, Descripcion = "gestionar estados de disputas" },
                new Permiso { Id = 22, Descripcion = "revisar reportes de actividad y seguridad" },
                new Permiso { Id = 23, Descripcion = "solucionar problemas de acceso y pagos" }
            };

            var relaciones = new List<Rol_Permiso>
            {
                // Administrador
                new Rol_Permiso { RolId = 1, PermisoId = 1 },
                new Rol_Permiso { RolId = 1, PermisoId = 2 },
                new Rol_Permiso { RolId = 1, PermisoId = 3 },
                new Rol_Permiso { RolId = 1, PermisoId = 4 },
                new Rol_Permiso { RolId = 1, PermisoId = 5 },
                new Rol_Permiso { RolId = 1, PermisoId = 6 },
                new Rol_Permiso { RolId = 1, PermisoId = 7 },

                // Subastador
                new Rol_Permiso { RolId = 2, PermisoId = 8 },
                new Rol_Permiso { RolId = 2, PermisoId = 9 },
                new Rol_Permiso { RolId = 2, PermisoId = 10 },
                new Rol_Permiso { RolId = 2, PermisoId = 11 },
                new Rol_Permiso { RolId = 2, PermisoId = 12 },
                new Rol_Permiso { RolId = 2, PermisoId = 13 },

                // Postor
                new Rol_Permiso { RolId = 3, PermisoId = 14 },
                new Rol_Permiso { RolId = 3, PermisoId = 15 },
                new Rol_Permiso { RolId = 3, PermisoId = 16 },
                new Rol_Permiso { RolId = 3, PermisoId = 17 },
                new Rol_Permiso { RolId = 3, PermisoId = 18 },
                new Rol_Permiso { RolId = 3, PermisoId = 19 },

                // Soporte Tecnico
                new Rol_Permiso { RolId = 4, PermisoId = 20 },
                new Rol_Permiso { RolId = 4, PermisoId = 21 },
                new Rol_Permiso { RolId = 4, PermisoId = 22 },
                new Rol_Permiso { RolId = 4, PermisoId = 23 }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.Permisos.AddRangeAsync(permisos);
            await context.Rol_Permisos.AddRangeAsync(relaciones);
            await context.SaveChangesAsync();
        }
    }
}
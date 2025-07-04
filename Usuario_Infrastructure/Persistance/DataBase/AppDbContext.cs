using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Usuario_Domain.Entities;

namespace Usuario_Infrastructure.Persistance.DataBase;

public class AppDbContext : DbContext
{
    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Actividad> Actividades { get; set; }

    public DbSet<Rol> Roles { get; set; }

    public DbSet<Permiso> Permisos { get; set; }

    public DbSet<Rol_Permiso> Rol_Permisos { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasKey(u => u.Id);

        modelBuilder.Entity<Actividad>().HasKey(a => a.Id);

        modelBuilder.Entity<Rol>().HasKey(r => r.Id);

        modelBuilder.Entity<Permiso>().HasKey(p => p.Id);

        modelBuilder.Entity<Rol_Permiso>().HasKey(rp => rp.Id);

        base.OnModelCreating(modelBuilder);
    }
}
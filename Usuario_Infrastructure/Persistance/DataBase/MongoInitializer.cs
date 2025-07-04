using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Usuario_Domain.Entities;

namespace Usuario_Infrastructure.Persistance.DataBase;

public class MongoInitializer
{
    private readonly IMongoClient _mongoClient;

    public MongoInitializer(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public void Initialize()
    {
        var database = _mongoClient.GetDatabase("usuarios_db");

        // Configurar colección de usuarios
        InitializeUsuarios(database);

        // Configurar colección de actividades
        InitializeActividades(database);

        // Configurar colección de roles
        Initialize_Roles(database);

        // Configurar colección de permisos
        Initialize_Permisos(database);

        // Configurar colección de roles y permisos
        Initialize_Roles_Permisos(database);
    }

    private void InitializeUsuarios(IMongoDatabase database)
    {
        var collection = database.GetCollection<Usuario_Mongo>("usuarios");

        // Índice único para correo
        var usuarioIndexKeys = Builders<Usuario_Mongo>.IndexKeys
            .Ascending(u => u.Correo);
        collection.Indexes.CreateOne(
            new CreateIndexModel<Usuario_Mongo>(usuarioIndexKeys,
                new CreateIndexOptions { Unique = true })
        );
    }

    private void InitializeActividades(IMongoDatabase database)
    {
        var collection = database.GetCollection<Actividad_Mongo>("actividades");

        // Índice compuesto para búsquedas por usuario y fecha
        var actividadIndexKeys = Builders<Actividad_Mongo>.IndexKeys
            .Ascending(a => a.UsuarioId)
            .Descending(a => a.Fecha);
        collection.Indexes.CreateOne(
            new CreateIndexModel<Actividad_Mongo>(actividadIndexKeys)
        );

    }

    private void Initialize_Roles(IMongoDatabase database)
    {
        var collection = database.GetCollection<Rol_Mongo>("roles");
        // Índice único para nombre de rol
        var rolIndexKeys = Builders<Rol_Mongo>.IndexKeys
            .Ascending(r => r.Nombre);
        collection.Indexes.CreateOne(
            new CreateIndexModel<Rol_Mongo>(rolIndexKeys,
                new CreateIndexOptions { Unique = true })
        );
    }

    private void Initialize_Permisos(IMongoDatabase database)
    {
        var collection = database.GetCollection<Permiso_Mongo>("permisos");
        // Índice único para nombre de permiso
        var permisoIndexKeys = Builders<Permiso_Mongo>.IndexKeys
            .Ascending(p => p.Descripcion);
        collection.Indexes.CreateOne(
            new CreateIndexModel<Permiso_Mongo>(permisoIndexKeys,
                new CreateIndexOptions { Unique = true })
        );
    }

    private void Initialize_Roles_Permisos(IMongoDatabase database)
    {
        var collection = database.GetCollection<Rol_Permiso>("roles_permisos");
        // Índice único para RolId y PermisoId
        var rolPermisoIndexKeys = Builders<Rol_Permiso>.IndexKeys
            .Ascending(rp => rp.RolId)
            .Ascending(rp => rp.PermisoId);
        collection.Indexes.CreateOne(
            new CreateIndexModel<Rol_Permiso>(rolPermisoIndexKeys,
                new CreateIndexOptions { Unique = true })
        );
    }
}
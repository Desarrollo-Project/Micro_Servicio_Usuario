using System.Reflection;
using System.Text;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Usuario_Domain.Interfaces;
using Usuario_Infrastructure.EventBus.Events;
using Usuario_Infrastructure.Persistance.DataBase;
using Usuario_Infrastructure.Persistance.Repositories;
using Usuario_Infrastructure.Persistance.Mongo;
using Producto.Infrastructure.EventBus.Consumer;
using Usuario_Application.Factorie;
using Usuario_Application.Services;
using Usuario_Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Microservicio Usuario", Version = "v1" });
});

// Mostrar errores detallados de JWT
IdentityModelEventSource.ShowPII = true;

// Autenticación con JWT Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakConfig = builder.Configuration.GetSection("Keycloak");
        var authority = $"{keycloakConfig["auth-server-url"]}/realms/{keycloakConfig["realm"]}";

        options.Authority = authority;
        options.Audience = keycloakConfig["ClientId"];
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var realmRolesClaim = context.Principal?.FindFirst("realm_access");
                    if (realmRolesClaim != null)
                    {
                        var parsed = System.Text.Json.JsonDocument.Parse(realmRolesClaim.Value);
                        if (parsed.RootElement.TryGetProperty("roles", out var roles))
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                identity.AddClaim(new Claim("permisos", role.GetString()));
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

// Autorización con políticas personalizadas
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequierePermisos", policy => policy.RequireClaim("permisos"));
});

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDB")));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase("usuarios_db");
});

// Inicializador Mongo
builder.Services.AddSingleton<MongoInitializer>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Usuario_Factory).Assembly));

// Servicios de aplicación
builder.Services.AddHttpClient();
builder.Services.AddScoped<IUsuarioFactory, Usuario_Factory>();
builder.Services.AddScoped<IKeycloak_Servicio, Keycloak_Servicio>();


// Repositorios y Mongo
builder.Services.AddScoped<IUsuario_Repository, Usuario_Repository>();
builder.Services.AddScoped<IActividad_Repository, ActividadRepository>();
builder.Services.AddScoped<IRol_Repositorio, Rol_Repositorio>();
builder.Services.AddScoped<IPermiso_Repositorio, Permiso_Repositorio>();

builder.Services.AddScoped<Mongo_Crear_Usuario>();
builder.Services.AddScoped<Mongo_Registrar_Actividad>();
builder.Services.AddScoped<Mongo_Actualizar_Perfil>();

builder.Services.AddSingleton<IMongoRepository<Usuario_Mongo>>(sp =>
    new MongoRepository<Usuario_Mongo>(sp.GetRequiredService<IMongoClient>(), "usuarios_db", "usuarios"));
builder.Services.AddSingleton<IMongoRepository<Actividad_Mongo>>(sp =>
    new MongoRepository<Actividad_Mongo>(sp.GetRequiredService<IMongoClient>(), "usuarios_db", "actividades"));
builder.Services.AddSingleton<IMongoRepository<Rol_Mongo>>(sp =>
    new MongoRepository<Rol_Mongo>(sp.GetRequiredService<IMongoClient>(), "usuarios_db", "roles"));
builder.Services.AddSingleton<IMongoRepository<Permiso_Mongo>>(sp =>
    new MongoRepository<Permiso_Mongo>(sp.GetRequiredService<IMongoClient>(), "usuarios_db", "permisos"));
builder.Services.AddSingleton<IMongoRepository<Rol_Permiso_Mongo>>(sp =>
    new MongoRepository<Rol_Permiso_Mongo>(sp.GetRequiredService<IMongoClient>(), "usuarios_db", "roles_permisos"));

// RabbitMQ Publisher
var rabbitHost = builder.Configuration["RabbitMQ:Host"];
var rabbitUser = builder.Configuration["RabbitMQ:Username"];
var rabbitPass = builder.Configuration["RabbitMQ:Password"];

builder.Services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>(_ =>
    new RabbitMQEventPublisher(rabbitHost, rabbitUser, rabbitPass));

// RabbitMQ Consumer
builder.Services.AddSingleton<IHostedService>(sp =>
    new Consumer_Event_Usuario_Creado(
        new RabbitMQEventConsumerConnection(rabbitHost, rabbitUser, rabbitPass),
        sp.GetRequiredService<IServiceProvider>()));


builder.Services.AddSingleton<IHostedService>(sp =>
    new Consumer_Event_Registrar_Actividad(
        new RabbitMQEventConsumerConnection(rabbitHost, rabbitUser, rabbitPass),
        sp.GetRequiredService<IServiceProvider>())
);

builder.Services.AddSingleton<IHostedService>(sp =>
    new Consumer_Event_Actualizar_Perfil(
        new RabbitMQEventConsumerConnection(rabbitHost, rabbitUser, rabbitPass),
        sp.GetRequiredService<IServiceProvider>())
);

builder.Services.AddSingleton<IHostedService>(sp =>
    new Consumer_Event_Cambiar_Password(
        new RabbitMQEventConsumerConnection(rabbitHost, rabbitUser, rabbitPass),
        sp.GetRequiredService<IServiceProvider>())
);

builder.Services.AddSingleton<IHostedService>(sp =>
    new Consumer_Event_Usuario_Confirmado(
        new RabbitMQEventConsumerConnection(rabbitHost, rabbitUser, rabbitPass),
        sp.GetRequiredService<IServiceProvider>())
);

// Comunicaciones otros Microservicios

builder.Services.AddHttpClient<INotificaciones_Cliente, Notificaciones_Cliente>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = config["ServiciosExternos:Notificaciones:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl);
});

// CORS para gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Usuarios API v1");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<Permiso_Middleware>();
app.UseAuthorization();
app.MapControllers();

// Inicialización Mongo
using (var scope = app.Services.CreateScope())
{
    var mongoInitializer = scope.ServiceProvider.GetRequiredService<MongoInitializer>();
    mongoInitializer.Initialize();
}

// Seed PostgreSQL
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.Run();
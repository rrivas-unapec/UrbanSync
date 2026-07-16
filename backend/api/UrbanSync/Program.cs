using UrbanSync.Business.Services;
using UrbanSync.DataAccess;
using UrbanSync.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("UrbanSyncDb")
    ?? throw new InvalidOperationException(
        "La cadena de conexión 'UrbanSyncDb' no está configurada."
    );

builder.Services.AddSingleton<IDbConnectionFactory>(
    new DbConnectionFactory(connectionString)
);

// Repositorios
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Servicios
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var app = builder.Build();

// Swagger disponible para verificar la API desplegada
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    application = "UrbanSync API",
    status = "running"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
}));

app.Run();
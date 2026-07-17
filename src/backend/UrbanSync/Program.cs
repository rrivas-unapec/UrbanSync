using UrbanSync.Business.Services;
using UrbanSync.DataAccess;
using UrbanSync.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Connection factory (una sola instancia con la connection string)
var connectionString = builder.Configuration.GetConnectionString("UrbanSyncDb")
    ?? throw new InvalidOperationException("Connection string 'UrbanSyncDb' no configurada.");
builder.Services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));



// Repositories (DataAccess)
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Services (Business)
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

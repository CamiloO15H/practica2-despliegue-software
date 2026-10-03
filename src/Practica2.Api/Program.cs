using Practica2.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Práctica 2 · API de Productos", Version = "v1" });
});
builder.Services.AddSingleton<ProductoRepository>();

var app = builder.Build();

// Swagger habilitado en TODOS los ambientes (no solo Development).
// Dentro del contenedor la app corre como "Production"; si esto quedara dentro de
// "if (app.Environment.IsDevelopment())", Swagger no aparecería en Docker ni en Kubernetes.
app.UseSwagger();
app.UseSwaggerUI();

// Sin UseHttpsRedirection: el contenedor solo expone HTTP en el puerto 8080.

app.MapControllers();

// Endpoint de salud usado por las sondas (probes) de Kubernetes.
app.MapGet("/health", () => Results.Ok(new
{
    estado = "ok",
    host = Environment.MachineName,   // muestra el nombre del contenedor/pod que respondió
    hora = DateTime.UtcNow
}));

// La raíz redirige a Swagger para facilitar la demostración.
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

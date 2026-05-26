using Envios.Domain.Entities;
using Envios.Domain.Enums;
using Envios.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ─────────────────────────────────────────────────
builder.Services.AddDbContext<EnviosDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("EnviosDB"),
        npgsql => npgsql.MigrationsAssembly("Envios.Infrastructure")
    )
);

// ── Swagger ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Envíos API",
        Version = "v1",
        Description = "Microservicio de gestión de envíos y rastreo"
    });
});

var app = builder.Build();

// ── Migraciones automáticas ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EnviosDbContext>();
    db.Database.Migrate();
}

// ── Middleware ────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Envíos API v1");
    c.RoutePrefix = "swagger";
});

app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var error = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = 500,
            error = "Error interno del servidor.",
            detalle = error?.Error.Message,
            inner = error?.Error.InnerException?.Message,  
            inner2 = error?.Error.InnerException?.InnerException?.Message,  
            timestamp = DateTime.UtcNow
        });
    });
});

// ══════════════════════════════════════════════════════════════════
// ENDPOINTS
// ══════════════════════════════════════════════════════════════════

// GET /health
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Envios API",
    timestamp = DateTime.UtcNow
}))
.WithTags("Health")
.WithSummary("Verificar estado del servicio");

app.MapPost("/api/envios", async (
    CrearEnvioRequest req,
    EnviosDbContext db) =>
{
    var fechaUtc = DateTime.SpecifyKind(req.FechaEstimada, DateTimeKind.Utc);

    // Buscar repartidor disponible
    var repartidor = await db.Repartidores
        .FirstOrDefaultAsync(r => r.EstaDisponible && r.Status == 1);

    string nombreRepartidor;
    string? telefonoRepartidor;
    int? idRepartidor;

    if (repartidor is not null)
    {
        nombreRepartidor = repartidor.Nombre;
        telefonoRepartidor = repartidor.Telefono;
        idRepartidor = repartidor.Id;
        repartidor.EstaDisponible = false;
    }
    else
    {
        nombreRepartidor = "Se entregará en la paquetería más cercana";
        telefonoRepartidor = null;
        idRepartidor = null;
    }

    var resultado = Envio.Crear(
        req.IdOrden,
        req.DireccionSnapshot,
        nombreRepartidor,
        telefonoRepartidor,
        idRepartidor,
        fechaUtc);

    if (!resultado.EsExitoso)
        return Results.BadRequest(new { error = resultado.Error });

    var envio = resultado.Valor!;
    db.Envios.Add(envio);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/envios/{envio.Id}",
        new
        {
            envio.Id,
            envio.EstadoActual,
            envio.GuiaPaqueteria,
            envio.NombreRepartidor,
            envio.TelefonoRepartidor,
            envio.FechaEntregado
        });
})
.WithName("CrearEnvio")
.WithTags("Envios")
.WithSummary("Crear un nuevo envío asociado a una orden");

// GET /api/envios/{id}
app.MapGet("/api/envios/{id:guid}", async (
    Guid id,
    EnviosDbContext db) =>
{
    var envio = await db.Envios
        .Include(e => e.HistorialRastros)
        .FirstOrDefaultAsync(e => e.Id == id);

    return envio is null
        ? Results.NotFound(new { error = "Envío no encontrado." })
        : Results.Ok(new
        {
            envio.Id,
            envio.IdOrden,
            envio.DireccionSnapshot,
            envio.GuiaPaqueteria,
            EstadoActual = envio.EstadoActual.ToString(),
            envio.NombreRepartidor,
            envio.FechaEstimada,
            envio.FechaEntregado
        });
})
.WithName("ObtenerEnvio")
.WithTags("Envios")
.WithSummary("Obtener detalle de un envío por su ID");

// GET /api/envios/orden/{idOrden}
app.MapGet("/api/envios/orden/{idOrden:int}", async (
    int idOrden,
    EnviosDbContext db) =>
{
    var envio = await db.Envios
        .FirstOrDefaultAsync(e => e.IdOrden == idOrden);

    return envio is null
        ? Results.NotFound(new { error = "No se encontró envío para esa orden." })
        : Results.Ok(new
        {
            envio.Id,
            envio.IdOrden,
            envio.DireccionSnapshot,
            envio.GuiaPaqueteria,
            EstadoActual = envio.EstadoActual.ToString(),
            envio.NombreRepartidor,
            envio.TelefonoRepartidor,  
            envio.FechaEstimada,
            envio.FechaEntregado
        });
})
.WithName("ObtenerEnvioPorOrden")
.WithTags("Envios")
.WithSummary("Obtener el envío asociado a una orden");

// PATCH /api/envios/{id}/estado
app.MapMethods("/api/envios/{id:guid}/estado", ["PATCH"], async (
    Guid id,
    CambiarEstadoRequest req,
    EnviosDbContext db) =>
{
    var envio = await db.Envios
        .Include(e => e.HistorialRastros)
        .FirstOrDefaultAsync(e => e.Id == id);
    if (envio is null)
        return Results.NotFound(new { error = "Envío no encontrado." });

    if (!Enum.TryParse<EstadoEnvio>(req.NuevoEstado, out var nuevoEstado))
        return Results.BadRequest(new
        {
            error = "Estado no válido.",
            valoresPermitidos = Enum.GetNames<EstadoEnvio>()
        });

    var resultado = envio.CambiarEstado(nuevoEstado, req.Nota);
    if (!resultado.EsExitoso)
        return Results.BadRequest(new
        {
            error = resultado.Error,
            valoresPermitidos = Enum.GetNames<EstadoEnvio>()
        });

    // Liberar repartidor cuando se entrega
    if (nuevoEstado == EstadoEnvio.Entregado && envio.IdRepartidor.HasValue)
    {
        var repartidor = await db.Repartidores
            .FirstOrDefaultAsync(r => r.Id == envio.IdRepartidor.Value);
        if (repartidor is not null)
            repartidor.EstaDisponible = true;
    }

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        envio.Id,
        EstadoActual = envio.EstadoActual.ToString(),
        envio.FechaEntregado
    });
})
.WithName("CambiarEstadoEnvio")
.WithTags("Envios")
.WithSummary("Cambiar el estado del envío");

// PATCH /api/envios/{id}/reprogramar
app.MapMethods("/api/envios/{id:guid}/reprogramar", ["PATCH"], async (
    Guid id,
    ReprogramarEnvioRequest req,
    EnviosDbContext db) =>
{
    var envio = await db.Envios
        .Include(e => e.HistorialRastros)
        .FirstOrDefaultAsync(e => e.Id == id);

    if (envio is null)
        return Results.NotFound(new { error = "Envío no encontrado." });

    var resultado = envio.Reprogramar(req.NuevaFecha, req.Nota);

    if (!resultado.EsExitoso)
        return Results.BadRequest(new { error = resultado.Error });

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        envio.Id,
        EstadoActual = envio.EstadoActual.ToString(),
        envio.FechaEstimada
    });
})
.WithName("ReprogramarEnvio")
.WithTags("Envios")
.WithSummary("Reprogramar una entrega fallida");

// GET /api/envios/{id}/rastreo
app.MapGet("/api/envios/{id:guid}/rastreo", async (
    Guid id,
    EnviosDbContext db) =>
{
    var envio = await db.Envios
        .Include(e => e.HistorialRastros)
        .FirstOrDefaultAsync(e => e.Id == id);

    if (envio is null)
        return Results.NotFound(new { error = "Envío no encontrado." });

    var historial = envio.HistorialRastros
        .OrderBy(h => h.FechaEvento)
        .Select(h => new
        {
            Estado = h.Estado.ToString(),
            h.Nota,
            h.FechaEvento
        });

    return Results.Ok(historial);
})
.WithName("ObtenerRastreo")
.WithTags("Envios")
.WithSummary("Obtener historial completo de rastreo");

app.Run();

// ── Records para requests ─────────────────────────────────────────
record CrearEnvioRequest(
    int IdOrden,  
    string DireccionSnapshot,
    string NombreRepartidor,
    DateTime FechaEstimada);

record CambiarEstadoRequest(
    string NuevoEstado,
    string? Nota);

record ReprogramarEnvioRequest(
    DateTime NuevaFecha,
    string? Nota);
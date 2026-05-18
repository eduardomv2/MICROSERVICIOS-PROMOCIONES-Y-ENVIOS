using Promociones.Domain.Entities;
using Promociones.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ─────────────────────────────────────────────────
builder.Services.AddDbContext<PromocionesDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PromocionesDB"),
        npgsql => npgsql.MigrationsAssembly("Promociones.Infrastructure")
    )
);

// ── Swagger ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Promociones API",
        Version = "v1",
        Description = "Microservicio de gestión de promociones, descuentos y MSI"
    });
});

var app = builder.Build();

// ── Migraciones automáticas ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PromocionesDbContext>();
    db.Database.Migrate();
}

// ── Middleware ────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Promociones API v1");
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
    service = "Promociones API",
    timestamp = DateTime.UtcNow
}))
.WithTags("Health")
.WithSummary("Verificar estado del servicio");

// GET /api/promociones
app.MapGet("/api/promociones", async (PromocionesDbContext db) =>
    Results.Ok(await db.Promociones
        .Include(p => p.ReglasCategorias)
        .Include(p => p.OpcionesMSI)
        .ToListAsync()))
.WithName("ObtenerPromociones")
.WithTags("Promociones")
.WithSummary("Listar todas las campañas");

// GET /api/promociones/activas
app.MapGet("/api/promociones/activas", async (PromocionesDbContext db) =>
{
    var promociones = await db.Promociones
        .Include(p => p.ReglasCategorias)
        .Include(p => p.OpcionesMSI)
        .ToListAsync();

    var activas = promociones.Where(p => p.EstaVigente()).ToList();
    return Results.Ok(activas);
})
.WithName("ObtenerPromocionesActivas")
.WithTags("Promociones")
.WithSummary("Listar solo campañas vigentes");

// GET /api/promociones/{id}
app.MapGet("/api/promociones/{id:guid}", async (
    Guid id,
    PromocionesDbContext db) =>
{
    var promocion = await db.Promociones
        .Include(p => p.ReglasCategorias)
        .Include(p => p.OpcionesMSI)
        .FirstOrDefaultAsync(p => p.Id == id);

    return promocion is null
        ? Results.NotFound(new { error = "Promoción no encontrada." })
        : Results.Ok(promocion);
})
.WithName("ObtenerPromocionPorId")
.WithTags("Promociones")
.WithSummary("Obtener detalle completo de una campaña");

// POST /api/promociones
app.MapPost("/api/promociones", async (
    CrearPromocionRequest req,
    PromocionesDbContext db) =>
{
    var resultado = Promocion.Crear(
        req.NombreCampana,
        req.PorcentajeDescuento,
        req.FechaInicio,
        req.FechaFin,
        req.EsIndefinido,
        req.MontoMinimoCompra);

    if (!resultado.EsExitoso)
        return Results.BadRequest(new { error = resultado.Error });

    var promocion = resultado.Valor!;
    db.Promociones.Add(promocion);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/promociones/{promocion.Id}",
        new { promocion.Id, promocion.NombreCampana, promocion.EsIndefinido });
})
.WithName("CrearPromocion")
.WithTags("Promociones")
.WithSummary("Crear una nueva campaña de descuento");

// DELETE /api/promociones/{id}
app.MapDelete("/api/promociones/{id:guid}", async (
    Guid id,
    PromocionesDbContext db) =>
{
    var promocion = await db.Promociones.FindAsync(id);

    if (promocion is null)
        return Results.NotFound(new { error = "Promoción no encontrada." });

    db.Promociones.Remove(promocion);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("EliminarPromocion")
.WithTags("Promociones")
.WithSummary("Eliminar una campaña");

// POST /api/promociones/{id}/reglas
app.MapPost("/api/promociones/{id:guid}/reglas", async (
    Guid id,
    AgregarReglaRequest req,
    PromocionesDbContext db) =>
{
    var promocion = await db.Promociones
        .Include(p => p.ReglasCategorias)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (promocion is null)
        return Results.NotFound(new { error = "Promoción no encontrada." });

    var resultado = promocion.AgregarReglaCategoria(
        req.IdCategoriaCatalogo,
        req.PorcentajeAplicable);

    if (!resultado.EsExitoso)
        return Results.BadRequest(new { error = resultado.Error });

    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/promociones/{id}/reglas",
        new { req.IdCategoriaCatalogo, req.PorcentajeAplicable });
})
.WithName("AgregarReglaCategoria")
.WithTags("Promociones")
.WithSummary("Agregar regla de descuento por categoría");

// GET /api/promociones/{id}/reglas
app.MapGet("/api/promociones/{id:guid}/reglas", async (
    Guid id,
    PromocionesDbContext db) =>
{
    var promocion = await db.Promociones
        .Include(p => p.ReglasCategorias)
        .FirstOrDefaultAsync(p => p.Id == id);

    return promocion is null
        ? Results.NotFound(new { error = "Promoción no encontrada." })
        : Results.Ok(promocion.ReglasCategorias);
})
.WithName("ObtenerReglasCategoria")
.WithTags("Promociones")
.WithSummary("Listar reglas de categoría de una campaña");

// POST /api/promociones/{id}/msi
app.MapPost("/api/promociones/{id:guid}/msi", async (
    Guid id,
    AgregarMSIRequest req,
    PromocionesDbContext db) =>
{
    var promocion = await db.Promociones
        .Include(p => p.OpcionesMSI)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (promocion is null)
        return Results.NotFound(new { error = "Promoción no encontrada." });

    var resultado = promocion.AgregarOpcionMSI(
        req.Meses,
        req.BancosParticipantes,
        req.MontoMinimoCompra);

    if (!resultado.EsExitoso)
        return Results.BadRequest(new { error = resultado.Error });

    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/promociones/{id}/msi",
        new { req.Meses, req.BancosParticipantes, req.MontoMinimoCompra });
})
.WithName("AgregarOpcionMSI")
.WithTags("Promociones")
.WithSummary("Agregar opción de meses sin intereses");

// GET /api/promociones/{id}/msi
app.MapGet("/api/promociones/{id:guid}/msi", async (
    Guid id,
    PromocionesDbContext db) =>
{
    var promocion = await db.Promociones
        .Include(p => p.OpcionesMSI)
        .FirstOrDefaultAsync(p => p.Id == id);

    return promocion is null
        ? Results.NotFound(new { error = "Promoción no encontrada." })
        : Results.Ok(promocion.OpcionesMSI);
})
.WithName("ObtenerOpcionesMSI")
.WithTags("Promociones")
.WithSummary("Listar opciones MSI de una campaña");

// POST /api/promociones/aplicar
app.MapPost("/api/promociones/aplicar", async (
    AplicarPromocionRequest req,
    PromocionesDbContext db) =>
{
    if (req.MontoTotal <= 0)
        return Results.BadRequest(new { error = "El monto total debe ser mayor a cero." });

    var promociones = await db.Promociones
        .Include(p => p.ReglasCategorias)
        .Include(p => p.OpcionesMSI)
        .ToListAsync();

    var activas = promociones.Where(p => p.EstaVigente()).ToList();

    decimal descuentoTotal = 0;
    var promocionesAplicadas = new List<object>();
    var opcionesMSI = new List<object>();

    foreach (var promo in activas)
    {
        // Verificar monto mínimo
        if (promo.MontoMinimoCompra.HasValue &&
            req.MontoTotal < promo.MontoMinimoCompra.Value)
            continue;

        // Aplicar reglas por categoría
        foreach (var item in req.Items)
        {
            var regla = promo.ReglasCategorias
                .FirstOrDefault(r => r.IdCategoriaCatalogo == item.IdCategoria);

            if (regla is not null)
            {
                var descuentoItem = item.PrecioUnitario *
                    item.Cantidad *
                    (regla.PorcentajeAplicable / 100);
                descuentoTotal += descuentoItem;
                promocionesAplicadas.Add(new
                {
                    promo.NombreCampana,
                    regla.PorcentajeAplicable,
                    descuentoItem
                });
            }
        }

        // Agregar opciones MSI disponibles
        foreach (var msi in promo.OpcionesMSI
            .Where(m => req.MontoTotal >= m.MontoMinimoCompra))
        {
            opcionesMSI.Add(new
            {
                msi.Meses,
                msi.BancosParticipantes,
                msi.MontoMinimoCompra
            });
        }
    }

    return Results.Ok(new
    {
        montoOriginal = req.MontoTotal,
        descuentoTotal = Math.Round(descuentoTotal, 2),
        montoFinal = Math.Round(req.MontoTotal - descuentoTotal, 2),
        promocionesAplicadas,
        opcionesMSI
    });
})
.WithName("AplicarPromocion")
.WithTags("Promociones")
.WithSummary("Calcular descuentos aplicables a un carrito");

app.Run();

// ── Records para requests ─────────────────────────────────────────
record CrearPromocionRequest(
    string NombreCampana,
    decimal PorcentajeDescuento,
    DateTime FechaInicio,
    DateTime? FechaFin,
    bool EsIndefinido,
    decimal? MontoMinimoCompra);

record AgregarReglaRequest(
    Guid IdCategoriaCatalogo,
    decimal PorcentajeAplicable);

record AgregarMSIRequest(
    int Meses,
    string BancosParticipantes,
    decimal MontoMinimoCompra);

record ItemCarritoRequest(
    Guid IdProducto,
    Guid IdCategoria,
    bool EsElectronica,
    decimal PrecioUnitario,
    int Cantidad);

record AplicarPromocionRequest(
    decimal MontoTotal,
    List<ItemCarritoRequest> Items);
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PuntoTres.Data;

var builder = WebApplication.CreateBuilder(args);

// Ajustes Npgsql (antes de Build)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

// MVC
builder.Services.AddControllersWithViews();

// DbContext (PostgreSQL con Npgsql)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Persistir las claves de Data Protection (antiforgery) en PostgreSQL
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("PuntoTres");

var app = builder.Build();

// Aplicar migraciones automáticamente SOLO en producción (Render)
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate(); // crea/actualiza el esquema al iniciar
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("Migrate error: " + ex);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SolucionesPreparadas}/{action=Index}/{id?}");

app.Run();

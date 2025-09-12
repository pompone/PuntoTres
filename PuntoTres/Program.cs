using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PuntoTres.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext (PostgreSQL con Npgsql)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cultura por defecto (decimales con coma y fechas dd/MM/yyyy)
var ci = new CultureInfo("es-AR"); // o "es-ES"
CultureInfo.DefaultThreadCurrentCulture = ci;
CultureInfo.DefaultThreadCurrentUICulture = ci;

var app = builder.Build();

// Aplicar migraciones automáticamente SOLO en producción (Render)
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // crea/actualiza el esquema al iniciar
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

//Localización (tiene que ir antes de routing)
app.UseRequestLocalization(new RequestCulture(ci));

app.UseStaticFiles();
app.UseRouting();
// app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SolucionPreparadas}/{action=Index}/{id?}");

app.Run();



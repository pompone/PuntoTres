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
var ci = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = ci;
CultureInfo.DefaultThreadCurrentUICulture = ci;

var app = builder.Build();

//  RequestLocalization con OPTIONS (no RequestCulture directo)
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(ci),
    SupportedCultures = new[] { ci },
    SupportedUICultures = new[] { ci }
};
app.UseRequestLocalization(localizationOptions);

// Migraciones SOLO en producción (Render)
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
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
    pattern: "{controller=SolucionPreparadas}/{action=Index}/{id?}");

app.Run();





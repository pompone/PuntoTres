using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PuntoTres.Data;
using PuntoTres.Models;

var builder = WebApplication.CreateBuilder(args);

// --- MVC + Razor Pages (UI de Identity) ---
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    // Libera las páginas de Identity (Login, Register, ForgotPassword, etc.)
    options.Conventions.AllowAnonymousToAreaFolder("Identity", "/Account");

    
});

// --- DbContext: SQLite en Development, Postgres en Production ---
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlite(builder.Configuration.GetConnectionString("DevSqlite")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// --- Identity + Roles ---
builder.Services.AddDefaultIdentity<AppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// --- DataProtection (para antiforgery, sesiones seguras) ---
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("PuntoTres");

// --- Políticas personalizadas + Fallback que exige login en toda la app ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Reactivos.ReadWrite", policy =>
        policy.RequireRole("Admin", "Labo"));

    options.AddPolicy("Reactivos.Delete", policy =>
        policy.RequireRole("Admin"));

    // Todo requiere estar autenticado (salvo lo que tenga AllowAnonymous)
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// --- Inicialización por entorno ---
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Crea la base si no existe (SQLite)
    db.Database.EnsureCreated();

    // Seed de usuarios y roles para DEV
    async Task SeedDev()
    {
        foreach (var r in new[] { "Admin", "Labo" })
            if (!await roleMgr.RoleExistsAsync(r)) await roleMgr.CreateAsync(new IdentityRole(r));

        async Task Ensure(string u, string e, string p, string role)
        {
            var user = await userMgr.FindByNameAsync(u);
            if (user == null)
            {
                user = new AppUser { UserName = u, Email = e, EmailConfirmed = true };
                await userMgr.CreateAsync(user, p);
            }
            if (!await userMgr.IsInRoleAsync(user, role))
                await userMgr.AddToRoleAsync(user, role);
        }

        await Ensure("admin", "admin@local", "Admin!123", "Admin");
        await Ensure("labo", "labo@local", "Labo!1234", "Labo");
    }
    await SeedDev();
}
else
{
    // --- Producción: PostgreSQL ---
    using var scope = app.Services.CreateScope();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Aplica migraciones
    db.Database.Migrate();

    // Crea roles si faltan
    foreach (var r in new[] { "Admin", "Labo" })
        if (!roleMgr.RoleExistsAsync(r).GetAwaiter().GetResult())
            roleMgr.CreateAsync(new IdentityRole(r)).GetAwaiter().GetResult();

    // Passwords desde variables de entorno (Render)
    var adminPass = config["SEED_ADMIN_PASSWORD"];
    var laboPass = config["SEED_LABO_PASSWORD"];
    if (string.IsNullOrWhiteSpace(adminPass) || string.IsNullOrWhiteSpace(laboPass))
        throw new Exception("Faltan SEED_ADMIN_PASSWORD y/o SEED_LABO_PASSWORD en las variables de entorno.");

    void EnsureProd(string u, string e, string p, string role)
    {
        var user = userMgr.FindByNameAsync(u).GetAwaiter().GetResult();
        if (user == null)
        {
            user = new AppUser { UserName = u, Email = e, EmailConfirmed = true };
            var res = userMgr.CreateAsync(user, p).GetAwaiter().GetResult();
            if (!res.Succeeded) throw new Exception(string.Join(" | ", res.Errors.Select(x => x.Description)));
        }
        if (!userMgr.IsInRoleAsync(user, role).GetAwaiter().GetResult())
            userMgr.AddToRoleAsync(user, role).GetAwaiter().GetResult();
    }

    EnsureProd("admin", "admin@local", adminPass, "Admin");
    EnsureProd("labo", "labo@local", laboPass, "Labo");
}

// --- Middleware pipeline ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// --- Rutas ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SolucionPreparadas}/{action=Index}/{id?}");

app.MapRazorPages(); // necesario para las páginas de Identity
app.MapGet("/Identity/Account/Register", () => Results.NotFound()).AllowAnonymous();
// --- Health Check (libre) ---
app.MapGet("/healthz", () => Results.Ok("OK")).AllowAnonymous();

// --- Run ---
app.Run();


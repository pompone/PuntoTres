using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoTres.Data;
using PuntoTres.Models;
using System.Globalization;
using System.Text;

namespace PuntoTres.Controllers
{
    [Authorize] // opcional: quitá si querés dejarlo libre
    public class BalanceController : Controller
    {
        private readonly AppDbContext _db;

        public BalanceController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(BalanceFiltroVM vm)
        {
            // Defaults de primer render
            vm.Desde ??= DateTime.Today.AddDays(-30);
            vm.Hasta ??= DateTime.Today;

            // Si aún no pusieron código, muestro solo el formulario
            if (string.IsNullOrWhiteSpace(vm.CodigoInterno))
                return View(vm);

            var desde = vm.Desde.Value.Date;
            var hasta = vm.Hasta.Value.Date.AddDays(1).AddTicks(-1); // incluye todo el día
            var code = vm.CodigoInterno.Trim();

            var q = _db.SolucionesPreparadas
                       .AsNoTracking()
                       .Where(s => s.CodigoInterno == code &&
                                   s.Fecha >= desde && s.Fecha <= hasta);

            vm.Registros = await q.CountAsync();

            // Fallback para SQLite: SUM(decimal) no soportado
            if (_db.Database.IsSqlite())
                vm.TotalUsado = q.AsEnumerable().Sum(s => s.CantidadBase ?? 0m);
            else
                vm.TotalUsado = decimal.Round(vm.TotalUsado, 4, MidpointRounding.AwayFromZero);

            return View(vm);
        }

        // Exportar detalle CSV compatible con Excel ES (sep ';', decimales con ',')
        [HttpGet]
        public async Task<IActionResult> ExportCsv(DateTime? desde, DateTime? hasta, string codigoInterno)
        {
            if (string.IsNullOrWhiteSpace(codigoInterno))
                return BadRequest("Debe especificar el código interno.");

            var desdeOk = (desde ?? DateTime.Today.AddDays(-30)).Date;
            var hastaOk = (hasta ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            var code = codigoInterno.Trim();

            var data = await _db.SolucionesPreparadas
                .AsNoTracking()
                .Where(s => s.CodigoInterno == code && s.Fecha >= desdeOk && s.Fecha <= hastaOk)
                .OrderBy(s => s.Fecha)
                .Select(s => new { s.Fecha, s.CodigoInterno, s.Nombre, s.CantidadBase })
                .ToListAsync();

            var culture = new CultureInfo("es-AR"); // coma decimal
            var sep = ";"; // separador de columnas amigable para Excel ES

            var header = $"Fecha{sep}Código interno{sep}Nombre{sep}Cantidad base (g/ml)";
            var lines = data.Select(d =>
                $"{d.Fecha:yyyy-MM-dd}{sep}{d.CodigoInterno}{sep}{d.Nombre}{sep}{(d.CantidadBase ?? 0m).ToString("0.####", culture)}");

            var csv = header + "\r\n" + string.Join("\r\n", lines);

            // BOM UTF-8 para que Excel abra acentos correctamente
            var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv);
            return File(bytes, "text/csv", $"balance_{code}_{DateTime.Now:yyyyMMddHHmm}.csv");
        }
    }
}
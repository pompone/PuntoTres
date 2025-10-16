using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoTres.Data;
using PuntoTres.Models;
using PuntoTres.Utils;

namespace PuntoTres.Controllers
{
    public class SolucionPreparadasController : Controller
    {
        private readonly AppDbContext _context;

        public SolucionPreparadasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SolucionPreparadas
        // /SolucionPreparadas?fechaInicio=2025-09-01&fechaFin=2025-09-30&pagina=1&tamanio=10
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin, int pagina = 1, int tamanio = 10)
        {
            var q = _context.SolucionesPreparadas
                .AsNoTracking()
                .AsQueryable();

            if (fechaInicio.HasValue)
                q = q.Where(s => s.Fecha >= fechaInicio.Value.Date);

            if (fechaFin.HasValue)
            {
                // Inclusivo hasta el final del día seleccionado
                var finExclusivo = fechaFin.Value.Date.AddDays(1);
                q = q.Where(s => s.Fecha < finExclusivo);
            }

            // Paginación (orden estable por Fecha desc, luego Id desc)
            var lista = await PaginatedList<SolucionPreparada>.CreateAsync(
                q.OrderByDescending(s => s.Fecha).ThenByDescending(s => s.Id),
                pagina,
                tamanio
            );

            // Para repoblar los inputs <input type="date"> y conservar parámetros en los links
            ViewData["fechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["fechaFin"]    = fechaFin?.ToString("yyyy-MM-dd");
            ViewData["Tamanio"]     = tamanio;
            ViewData["Pagina"]      = pagina;

            return View(lista);
        }

        // GET: SolucionPreparadas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var solucionPreparada = await _context.SolucionesPreparadas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solucionPreparada == null) return NotFound();

            return View(solucionPreparada);
        }

        // GET: SolucionPreparadas/Create
        public IActionResult Create(DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Guardamos rango para volver con RedirectToAction después del POST
            ViewData["fechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["fechaFin"]    = fechaFin?.ToString("yyyy-MM-dd");
            return View();
        }

        // POST: SolucionPreparadas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolucionPreparada solucionPreparada, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (!ModelState.IsValid)
            {
                ViewData["fechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
                ViewData["fechaFin"]    = fechaFin?.ToString("yyyy-MM-dd");
                return View(solucionPreparada);
            }

            _context.Add(solucionPreparada);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { fechaInicio, fechaFin });
        }

        // GET: SolucionPreparadas/Edit/5
        public async Task<IActionResult> Edit(int? id, DateTime? fechaInicio, DateTime? fechaFin, int pagina = 1, int tamanio = 10)
        {
            if (id == null) return NotFound();

            var solucionPreparada = await _context.SolucionesPreparadas.FindAsync(id);
            if (solucionPreparada == null) return NotFound();

            ViewData["fechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["fechaFin"]    = fechaFin?.ToString("yyyy-MM-dd");
            ViewData["Tamanio"]     = tamanio;
            ViewData["Pagina"]      = pagina;

            return View(solucionPreparada);
        }

        // POST: SolucionPreparadas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SolucionPreparada solucionPreparada, DateTime? fechaInicio, DateTime? fechaFin, int pagina = 1, int tamanio = 10)
        {
            if (id != solucionPreparada.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["fechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
                ViewData["fechaFin"]    = fechaFin?.ToString("yyyy-MM-dd");
                ViewData["Tamanio"]     = tamanio;
                ViewData["Pagina"]      = pagina;
                return View(solucionPreparada);
            }

            try
            {
                _context.Update(solucionPreparada);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SolucionPreparadaExists(solucionPreparada.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index), new { fechaInicio, fechaFin, pagina, tamanio });
        }

        // GET: SolucionPreparadas/Delete/5
        public async Task<IActionResult> Delete(int? id, DateTime? fechaInicio, DateTime? fechaFin, int pagina = 1, int tamanio = 10)
        {
            if (id == null) return NotFound();

            var solucionPreparada = await _context.SolucionesPreparadas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (solucionPreparada == null) return NotFound();

            ViewData["fechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["fechaFin"]    = fechaFin?.ToString("yyyy-MM-dd");
            ViewData["Tamanio"]     = tamanio;
            ViewData["Pagina"]      = pagina;

            return View(solucionPreparada);
        }

        // POST: SolucionPreparadas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, DateTime? fechaInicio, DateTime? fechaFin, int pagina = 1, int tamanio = 10)
        {
            var solucionPreparada = await _context.SolucionesPreparadas.FindAsync(id);
            if (solucionPreparada != null)
            {
                _context.SolucionesPreparadas.Remove(solucionPreparada);
                await _context.SaveChangesAsync();
            }

            // Volver al Index conservando filtros y página
            return RedirectToAction(nameof(Index), new { fechaInicio, fechaFin, pagina, tamanio });
        }

        private bool SolucionPreparadaExists(int id)
            => _context.SolucionesPreparadas.Any(e => e.Id == id);
    }
}






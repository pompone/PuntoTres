using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoTres.Data;
using PuntoTres.Models;

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
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
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

            var lista = await q
                .OrderBy(s => s.Fecha)
                .ToListAsync();

            // Para repoblar los inputs <input type="date">
            ViewData["fechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["fechaFin"] = fechaFin?.ToString("yyyy-MM-dd");

            return View(lista);
        }

        // GET: SolucionPreparadas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var solucionPreparada = await _context.SolucionesPreparadas
                .FirstOrDefaultAsync(m => m.Id == id);

            if (solucionPreparada == null) return NotFound();

            return View(solucionPreparada);
        }

        // GET: SolucionPreparadas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SolucionPreparadas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fecha,CodigoInterno,Marca,Nombre,CantidadBase,VolumenFinal,Lote,ConcentracionObtenida,IdReactivo,FechaVencimiento")] SolucionPreparada solucionPreparada)
        {
            if (!ModelState.IsValid)
                return View(solucionPreparada);

            try
            {
                _context.Add(solucionPreparada);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Log en Render (Logs)
                Console.WriteLine(ex);

                // Mensaje visible en la vista
                ModelState.AddModelError(string.Empty, $"Error al guardar: {ex.InnerException?.Message ?? ex.Message}");
                return View(solucionPreparada);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
                return View(solucionPreparada);
            }
        }

        // GET: SolucionPreparadas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var solucionPreparada = await _context.SolucionesPreparadas.FindAsync(id);
            if (solucionPreparada == null) return NotFound();

            return View(solucionPreparada);
        }

        // POST: SolucionPreparadas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fecha,CodigoInterno,Marca,Nombre,CantidadBase,VolumenFinal,Lote,ConcentracionObtenida,IdReactivo,FechaVencimiento")] SolucionPreparada solucionPreparada)
        {
            if (id != solucionPreparada.Id) return NotFound();

            if (!ModelState.IsValid) return View(solucionPreparada);

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

            return RedirectToAction(nameof(Index));
        }

        // GET: SolucionPreparadas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var solucionPreparada = await _context.SolucionesPreparadas
                .FirstOrDefaultAsync(m => m.Id == id);

            if (solucionPreparada == null) return NotFound();

            return View(solucionPreparada);
        }

        // POST: SolucionPreparadas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solucionPreparada = await _context.SolucionesPreparadas.FindAsync(id);
            if (solucionPreparada != null)
                _context.SolucionesPreparadas.Remove(solucionPreparada);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SolucionPreparadaExists(int id)
        {
            return _context.SolucionesPreparadas.Any(e => e.Id == id);
        }
    }
}


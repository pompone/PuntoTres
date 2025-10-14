using System;
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
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin, int page = 1, int pageSize = 15)
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

            int totalRegistros = await q.CountAsync();

            var lista = await q
                .OrderByDescending(s => s.Fecha)   // más nuevo primero
                .ThenByDescending(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Para repoblar los inputs <input type="date">
            ViewData["fechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["fechaFin"] = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRegistros / (double)pageSize);

            return View(lista);
        }

        //  Exportar a PDF con el filtro aplicado
        public async Task<IActionResult> DescargarPDF(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var q = _context.SolucionesPreparadas
                .AsNoTracking()
                .AsQueryable();

            if (fechaInicio.HasValue)
                q = q.Where(s => s.Fecha >= fechaInicio.Value.Date);

            if (fechaFin.HasValue)
            {
                var finExclusivo = fechaFin.Value.Date.AddDays(1);
                q = q.Where(s => s.Fecha < finExclusivo);
            }

            var lista = await q
                .OrderByDescending(s => s.Fecha)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

            // Si no usás QuestPDF todavía, podés implementar este método luego
            var pdfBytes = GenerarPdf(lista, fechaInicio, fechaFin);
            return File(pdfBytes, "application/pdf", "SolucionesPreparadas.pdf");
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
        public IActionResult Create() => View();

        // POST: SolucionPreparadas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fecha,CodigoInterno,Marca,Nombre,CantidadBase,VolumenFinal,Lote,ConcentracionObtenida,IdReactivo,FechaVencimiento")] SolucionPreparada solucionPreparada)
        {
            if (!ModelState.IsValid) return View(solucionPreparada);

            _context.Add(solucionPreparada);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
                if (!SolucionPreparadaExists(solucionPreparada.Id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: SolucionPreparadas/Delete/5
        public async Task<IActionResult> Delete(int? id, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (id == null) return NotFound();

            var solucionPreparada = await _context.SolucionesPreparadas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (solucionPreparada == null) return NotFound();

            // Guardar rango para devolverlo luego
            ViewData["fechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["fechaFin"] = fechaFin?.ToString("yyyy-MM-dd");

            return View(solucionPreparada);
        }

        // POST: SolucionPreparadas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var sp = await _context.SolucionesPreparadas.FindAsync(id);
            if (sp != null)
            {
                _context.SolucionesPreparadas.Remove(sp);
                await _context.SaveChangesAsync();
            }

            // Volver al Index conservando el rango (si la vista lo envía)
            return RedirectToAction(nameof(Index), new { fechaInicio, fechaFin });
        }

        private bool SolucionPreparadaExists(int id)
            => _context.SolucionesPreparadas.Any(e => e.Id == id);

        // Método auxiliar para generar PDF (estructura base)
        private byte[] GenerarPdf(List<SolucionPreparada> lista, DateTime? inicio, DateTime? fin)
        {
            // En este punto podés usar QuestPDF, iTextSharp o similar
            // Por ahora, devolvemos un PDF mínimo de prueba:

            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms))
            {
                writer.WriteLine($"Soluciones preparadas del {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}");
                writer.WriteLine("------------------------------------------------");
                foreach (var s in lista)
                {
                    writer.WriteLine($"{s.Fecha:dd/MM/yyyy} - {s.Nombre} ({s.Lote})");
                }
                writer.Flush();
            }

            // Retornar un pseudo-PDF (texto plano por ahora)
            return ms.ToArray();
        }
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoTres.Data;
using PuntoTres.Models;

//  QuestPDF (para generar el PDF real)
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
                .OrderByDescending(s => s.Fecha)
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

        // Exportar a PDF con el filtro aplicado
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

        //  Método auxiliar para generar PDF con QuestPDF
        private byte[] GenerarPdf(List<SolucionPreparada> lista, DateTime? inicio, DateTime? fin)
        {
            var desde = inicio?.ToString("dd/MM/yyyy") ?? "—";
            var hasta = fin?.ToString("dd/MM/yyyy") ?? "—";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Encabezado
                    page.Header()
                        .Text("Informe de soluciones preparadas")
                        .FontSize(16)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2)
                        .AlignCenter();

                    // Contenido
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Rango aplicado: {desde} - {hasta}")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken2)
                            .PaddingBottom(8);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); // Fecha
                                columns.RelativeColumn(2); // Nombre
                                columns.RelativeColumn(1); // Lote
                                columns.RelativeColumn(1); // Volumen
                                columns.RelativeColumn(1); // Concentración
                            });

                            // Encabezado de tabla
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Fecha").SemiBold();
                                header.Cell().Element(CellStyle).Text("Nombre").SemiBold();
                                header.Cell().Element(CellStyle).Text("Lote").SemiBold();
                                header.Cell().Element(CellStyle).Text("Volumen (ml)").SemiBold();
                                header.Cell().Element(CellStyle).Text("Conc. obtenida").SemiBold();
                            });

                            // Filas de datos
                            foreach (var s in lista)
                            {
                                table.Cell().Element(CellStyle).Text(s.Fecha.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(s.Nombre ?? "");
                                table.Cell().Element(CellStyle).Text(s.Lote ?? "");
                                table.Cell().Element(CellStyle).Text($"{s.VolumenFinal:0.##}");
                                table.Cell().Element(CellStyle).Text($"{s.ConcentracionObtenida}");
                            }

                            static IContainer CellStyle(IContainer container) =>
                                container.BorderBottom(0.5f)
                                         .BorderColor(Colors.Grey.Lighten2)
                                         .PaddingVertical(4)
                                         .PaddingHorizontal(3);
                        });
                    });

                    // Pie de página
                    page.Footer()
                        .AlignRight()
                        .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(9);
                });
            });

            return document.GeneratePdf();
        }
    }
}




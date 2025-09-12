using Microsoft.EntityFrameworkCore;
using PuntoTres.Models;

namespace PuntoTres.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SolucionPreparada> SolucionesPreparadas { get; set; }
    }
}

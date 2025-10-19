using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PuntoTres.Models;

namespace PuntoTres.Data
{
    // Ahora hereda de IdentityDbContext<AppUser> e implementa IDataProtectionKeyContext
    public class AppDbContext : IdentityDbContext<AppUser>, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tus tablas
        public DbSet<SolucionPreparada> SolucionesPreparadas => Set<SolucionPreparada>();

        // Claves de Data Protection (si las seguís guardando en BD)
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ¡IMPORTANTE para que Identity configure sus tablas!
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SolucionPreparada>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).HasMaxLength(200);
                entity.Property(e => e.CodigoInterno).HasMaxLength(50);
                entity.Property(e => e.Marca).HasMaxLength(100);
                entity.Property(e => e.Lote).HasMaxLength(100);
                entity.Property(e => e.IdReactivo).HasMaxLength(100);
                entity.Property(e => e.ConcentracionObtenida).HasMaxLength(100);

                entity.Property(e => e.Fecha).HasColumnType("date");
                entity.Property(e => e.FechaVencimiento).HasColumnType("date");

                entity.Property(e => e.CantidadBase).HasPrecision(18, 4);
                entity.Property(e => e.VolumenFinal).HasPrecision(18, 4);
            });
        }
    }
}

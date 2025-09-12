using Microsoft.EntityFrameworkCore;
using PuntoTres.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace PuntoTres.Data
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SolucionPreparada> SolucionesPreparadas => Set<SolucionPreparada>();

        //NUEVO: tabla donde EF guardará las claves
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

                entity.Property(e => e.CantidadBase).HasPrecision(18, 2);
                entity.Property(e => e.VolumenFinal).HasPrecision(18, 2);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

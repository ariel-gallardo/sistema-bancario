using Microsoft.EntityFrameworkCore;
using Pagos.Domain;

namespace Pagos.Data;

public class PagosDbContext(DbContextOptions<PagosDbContext> options) : DbContext(options)
{
    public DbSet<PagoProgramado> PagosProgramados => Set<PagoProgramado>();
    public DbSet<PagoHistorico> PagosHistoricos => Set<PagoHistorico>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PagoProgramado>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Empresa).HasMaxLength(160).IsRequired();
            builder.Property(p => p.Descripcion).HasMaxLength(200).IsRequired();
            builder.Property(p => p.Moneda).HasMaxLength(4).IsRequired();
            builder.Property(p => p.Estado).HasMaxLength(32).IsRequired();
            builder.Property(p => p.Importe).HasPrecision(18, 2);
            builder.HasIndex(p => new { p.ClienteId, p.FechaProgramada });
        });

        modelBuilder.Entity<PagoHistorico>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Empresa).HasMaxLength(160).IsRequired();
            builder.Property(p => p.Categoria).HasMaxLength(80).IsRequired();
            builder.Property(p => p.Moneda).HasMaxLength(4).IsRequired();
            builder.Property(p => p.MedioPago).HasMaxLength(64).IsRequired();
            builder.Property(p => p.Importe).HasPrecision(18, 2);
            builder.HasIndex(p => new { p.ClienteId, p.FechaPago });
        });
    }
}

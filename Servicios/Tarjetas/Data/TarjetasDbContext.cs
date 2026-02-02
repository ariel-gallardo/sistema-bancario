using Microsoft.EntityFrameworkCore;
using Tarjetas.Domain;

namespace Tarjetas.Data;

public class TarjetasDbContext(DbContextOptions<TarjetasDbContext> options) : DbContext(options)
{
    public DbSet<TarjetaCredito> Tarjetas => Set<TarjetaCredito>();
    public DbSet<MovimientoTarjeta> Movimientos => Set<MovimientoTarjeta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TarjetaCredito>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.NumeroEnmascarado).HasMaxLength(32).IsRequired();
            builder.Property(t => t.Marca).HasMaxLength(40).IsRequired();
            builder.Property(t => t.Limite).HasPrecision(18, 2);
            builder.Property(t => t.SaldoUtilizado).HasPrecision(18, 2);
            builder.Property(t => t.PagoMinimo).HasPrecision(18, 2);
        });

        modelBuilder.Entity<MovimientoTarjeta>(builder =>
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Comercio).HasMaxLength(120).IsRequired();
            builder.Property(m => m.Descripcion).HasMaxLength(200).IsRequired();
            builder.Property(m => m.Categoria).HasMaxLength(40).IsRequired();
            builder.Property(m => m.Importe).HasPrecision(18, 2);
            builder.HasIndex(m => new { m.ClienteId, m.Fecha });
            builder.HasOne(m => m.Tarjeta)
                .WithMany(t => t.Movimientos)
                .HasForeignKey(m => m.TarjetaId);
        });
    }
}

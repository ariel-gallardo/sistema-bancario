using Cuentas.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cuentas.Data;

public class CuentasDbContext(DbContextOptions<CuentasDbContext> options) : DbContext(options)
{
    public DbSet<CuentaBancaria> Cuentas => Set<CuentaBancaria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuentaBancaria>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Alias).HasMaxLength(80).IsRequired();
            builder.Property(c => c.Banco).HasMaxLength(120).IsRequired();
            builder.Property(c => c.Numero).HasMaxLength(34).IsRequired();
            builder.Property(c => c.Moneda).HasMaxLength(4).IsRequired();
            builder.Property(c => c.SaldoActual).HasPrecision(18, 2);
            builder.Property(c => c.LimiteDescubierto).HasPrecision(18, 2);
            builder.HasIndex(c => new { c.ClienteId, c.EsPrincipal });
        });
    }
}

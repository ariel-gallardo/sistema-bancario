using Clientes.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Data;

public class ClientesDbContext(DbContextOptions<ClientesDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ClienteRegistro> Registros => Set<ClienteRegistro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.NombreCompleto).HasMaxLength(160).IsRequired();
            builder.Property(c => c.Documento).HasMaxLength(32).IsRequired();
            builder.Property(c => c.Email).HasMaxLength(160).IsRequired();
            builder.HasIndex(c => c.Email).IsUnique();
            builder.Property(c => c.PasswordHash).HasMaxLength(512).IsRequired();
            builder.Property(c => c.EsAdministrador).HasDefaultValue(false);
        });

        modelBuilder.Entity<ClienteRegistro>(builder =>
        {
            builder.ToTable("ClienteRegistros");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.NombreCompleto).HasMaxLength(160).IsRequired();
            builder.Property(r => r.Documento).HasMaxLength(32).IsRequired();
            builder.Property(r => r.Email).HasMaxLength(160).IsRequired();
            builder.HasIndex(r => r.Email);
            builder.Property(r => r.Telefono).HasMaxLength(40).IsRequired();
            builder.Property(r => r.PasswordHash).HasMaxLength(512).IsRequired();
            builder.Property(r => r.Estado).HasMaxLength(32).HasDefaultValue("Pendiente");
            builder.Property(r => r.CreadoEnUtc).IsRequired();
        });
    }
}

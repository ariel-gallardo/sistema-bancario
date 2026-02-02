using Clientes.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clientes.Data;

public class ClientesDbContext(DbContextOptions<ClientesDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

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
        });
    }
}

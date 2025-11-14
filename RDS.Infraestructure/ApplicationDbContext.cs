// csharp
using Microsoft.EntityFrameworkCore;
using RDS.Core.Entities;

namespace RDS.Infraestructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ShortUrl> ShortUrls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração para ShortUrl
        modelBuilder.Entity<ShortUrl>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.LongUrl)
                .IsRequired()
                .HasMaxLength(2048); // Define o tamanho máximo da string

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetimeoffset"); // Define o tipo de coluna no banco

            entity.Property(e => e.ClickCount)
                .IsRequired()
                .HasDefaultValue(0); // Define um valor padrão
        });
    }
}
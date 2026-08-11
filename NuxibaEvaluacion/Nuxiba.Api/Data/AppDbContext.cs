using Microsoft.EntityFrameworkCore;
using Nuxiba.Api.Models;

namespace Nuxiba.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Login> Logins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Login>(entity =>
        {
            entity.ToTable("ccloglogin");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .HasColumnName("User_id");

            entity.Property(e => e.Extension)
                .HasColumnName("Extension");

            entity.Property(e => e.TipoMov)
                .HasColumnName("TipoMov");

            entity.Property(e => e.Fecha)
                .HasColumnName("fecha");
        });
    }
}
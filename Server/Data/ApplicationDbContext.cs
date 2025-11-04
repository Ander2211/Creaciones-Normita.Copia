using BCrypt.Net;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Shared.Models; // ✅ importar correctamente


namespace CreacionesNormita.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                // Campos obligatorios
                entity.Property(u => u.Dui).IsRequired();
                entity.Property(u => u.UserName).IsRequired();
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.PhoneNumber).IsRequired();
                entity.Property(u => u.Nombre).IsRequired();
                entity.Property(u => u.Pais).IsRequired();
                entity.Property(u => u.FechaNac).IsRequired();
                entity.Property(u => u.FechaCreacion).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();

                // Índices únicos
                entity.HasIndex(u => u.Dui).IsUnique();
                entity.HasIndex(u => u.UserName).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
            });

            // Map ImagenUrls (List<string>) to the existing DB column 'ImagenUrl' as JSON text
            builder.Entity<Shared.Models.Producto>(entity =>
            {
                // Use a JSON converter to store the list in the existing text column
                var jsonOptions = new System.Text.Json.JsonSerializerOptions();
                var converter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<List<string>, string>(
                    v => System.Text.Json.JsonSerializer.Serialize(v, jsonOptions),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
                );

                entity.Property(p => p.ImagenUrls)
                      .HasColumnName("ImagenUrl")
                      .HasColumnType("text")
                      .HasConversion(converter);
            });
        }

        // ✅ ya no hace falta poner el namespace completo
        public DbSet<Producto> Productos { get; set; }
    }


} 

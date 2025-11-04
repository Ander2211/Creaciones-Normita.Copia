using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Shared.Models
{
    public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripci�n es obligatoria")]
        [StringLength(500, ErrorMessage = "La descripci�n no puede superar los 500 caracteres")]
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0.01, 9999, ErrorMessage = "El precio debe ser mayor a 0")]
        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }

    // Ahora soportamos hasta 3 imágenes como un array de URLs.
    // Guardamos en la base de datos como JSON (jsonb en PostgreSQL).
    [Column(TypeName = "jsonb")]
    [JsonPropertyName("imagenUrls")]
    public List<string> ImagenUrls { get; set; } = new List<string>();

        [JsonPropertyName("activo")]
        public bool Activo { get; set; } = true;

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}


using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CreacionesNormita.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Shared.Models;

namespace CreacionesNormita.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProductosController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/productos
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<Producto>>> GetProductos([FromQuery] int pagina = 1, [FromQuery] int porPagina = 9)
        {
     
            var query = _context.Productos.Where(p => p.Activo).AsQueryable();
            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.FechaCreacion)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            var response = new PaginatedResponse<Producto>(items, totalItems, pagina, porPagina);
            return response;
        }

        // GET: api/productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null || !producto.Activo)
            {
                return NotFound();
            }

            return producto;
        }
        
        // POST: api/productos
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            // ✅ Forzar fecha de creación en el servidor
            producto.FechaCreacion = DateTime.UtcNow;

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
        }

        // PUT: api/productos/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            if (id != producto.Id)
            {
                return BadRequest();
            }

            // ✅ No permitir que se sobrescriba la fecha de creación
            var productoExistente = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (productoExistente == null) return NotFound();

            producto.FechaCreacion = productoExistente.FechaCreacion;

            _context.Entry(producto).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/productos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/productos/upload-image
        // Ahora acepta uno o varios archivos y devuelve { urls: [..] }
        [HttpPost("upload-image")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UploadImage(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No se subió ningún archivo.");

            var urls = new List<string>();

            foreach (var file in files.Take(3)) // límite práctico: 3 imágenes
            {
                if (file == null || file.Length == 0) continue;

                await using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "productos"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                urls.Add(uploadResult.SecureUrl.ToString());
            }

            return Ok(new { urls });
        }
    }
}
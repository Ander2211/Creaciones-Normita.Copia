using System.Net.Http.Json;
using Shared.Models;

namespace Client.Services
{
    public class ProductoService
    {
        private readonly HttpClient _http;

        public ProductoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Producto>> GetProductosAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<PaginatedResponse<Producto>>("api/productos");
                return response?.Items ?? new List<Producto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener productos: {ex.Message}");
                return new List<Producto>();
            }
        }
    }
}

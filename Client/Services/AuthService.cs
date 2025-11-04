using System.Net.Http.Json;
using Shared.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<HttpResponseMessage> LoginAsync(LoginRequest loginRequest)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/login")
            {
                Content = JsonContent.Create(loginRequest)
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include); // Asegura cookie

            return await _http.SendAsync(request);
        }
    }
}
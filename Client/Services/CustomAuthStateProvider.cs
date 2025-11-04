using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Models;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;

        public CustomAuthStateProvider(HttpClient http)
        {
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<UserDto>();
                    if (profile == null)
                    {
                        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                        return new AuthenticationState(anonymous);
                    }

                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, profile.UserName ?? string.Empty)
                    };

                    if (profile.Roles != null)
                    {
                        foreach (var role in profile.Roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(profile.AuthToken))
                    {
                        claims.Add(new Claim("AuthToken", profile.AuthToken));
                    }

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var user = new ClaimsPrincipal(identity);

                    return new AuthenticationState(user);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // 🔹 No hay sesión => usuario anónimo
                    var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                    return new AuthenticationState(anonymous);
                }
                else
                {
                    throw new Exception("Error inesperado al obtener perfil");
                }
            }
            catch
            {
                // 🔹 Error de red o servidor => usuario anónimo
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(anonymous);
            }
        }


        // Método para forzar notificación de cambios al iniciar/cerrar sesión
        public void NotifyUserAuthentication(string userName)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName)
            }, "serverAuth");

            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        }
    }
}


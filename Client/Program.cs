using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Services;
using Microsoft.AspNetCore.Components.Authorization; // ✅ importa tu servicio

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Registro del servicio de productos
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<ToastService>();

// HttpClient para consumir la API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7057/"),
    DefaultRequestVersion = new Version(2, 0)
});

// 🔑 Autorización y CustomAuthStateProvider
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthGuardService>();
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();

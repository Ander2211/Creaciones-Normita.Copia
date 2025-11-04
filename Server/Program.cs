using CloudinaryDotNet; //  importar
using CreacionesNormita.Server.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Server.Models;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Conexi�n a PostgreSQL con EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.Write($"[DEBUG]ConnectionStreamReader: '{connectionString}'");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrar controladores
builder.Services.AddControllers();

// Configurar CORS para permitir llamadas desde Client (Blazor) y Swagger (Server)
// Activar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
var corsPolicy = "_allowClientAndSwagger";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy,
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:5285", // Blazor Client
                "https://localhost:7175",  // Cliente Blazor HTTPS
                "http://localhost:5123",  // Swagger en el Server
                "https://localhost:7057"  // Swagger en el Server
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

//Inicio de la configuracion de JWT
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configuraci�n JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado.");
var key = Encoding.UTF8.GetBytes(secret);

// Servicios
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Login", Version = "v1" });

    // Bearer token para Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce tu token JWT. Ejemplo: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// Configurar autenticaci�n JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role
    };

    // Aqu� leemos el token desde la cookie si existe
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Token = context.Request.Cookies["AuthToken"];
            }
            return Task.CompletedTask;
        }
    };
});

// Autorizaci�n
builder.Services.AddAuthorization();
// Configurar Cloudinary
var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
Console.WriteLine($"[Cloudinary Config] Name={cloudinaryConfig["CloudName"]}, Key={cloudinaryConfig["ApiKey"]}, Secret={cloudinaryConfig["ApiSecret"]}");

var account = new Account(
    cloudinaryConfig["CloudName"],   // 
    cloudinaryConfig["ApiKey"],      // 
    cloudinaryConfig["ApiSecret"]    // 
);
var cloudinary = new Cloudinary(account);
builder.Services.AddSingleton(cloudinary);
builder.Services.AddScoped<Server.Services.JwtService>();
builder.Services.AddScoped<Server.Services.UserRegistrationService>();

var app = builder.Build();


// Configuraci�n del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicy);

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


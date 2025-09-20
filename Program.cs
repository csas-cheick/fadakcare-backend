using System.Text.Json.Serialization;
using System.Text.Json;
using backend.Data;
using backend.IServices;
using backend.Services;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La chaîne de connexion à la base de données est introuvable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.AllowAnyOrigin() //        .WithOrigins("http://localhost:3000")

                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQuestionnaireService, QuestionnaireService>();
builder.Services.AddScoped<IMedecinService, MedecinService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAlerteService, AlerteService>();
builder.Services.AddScoped<IConseilService, ConseilService>();
builder.Services.AddScoped<IIAService, IAService>();
builder.Services.AddScoped<IDepistageService, DepistageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IRendezVousService, RendezVousService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ITelemedicineService, TelemedicineService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrer 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new string[] {}
        }
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // camelCase pour correspondre au frontend
    });

// JWT Authentication configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");
var jwtIssuer = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT Key manquante dans la configuration.");

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

var app = builder.Build();
app.Urls.Add("http://0.0.0.0:5120");

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Endpoint de debug pour vérifier les claims du token (à retirer en prod)
app.MapGet("/api/debug/me", (HttpContext ctx) =>
{
    if (!ctx.User.Identity?.IsAuthenticated ?? true)
    {
        return Results.Unauthorized();
    }
    var claims = ctx.User.Claims.Select(c => new { c.Type, c.Value });
    return Results.Ok(new {
        Authenticated = true,
        Name = ctx.User.Identity?.Name,
        Claims = claims
    });
}).RequireAuthorization();

app.Run();


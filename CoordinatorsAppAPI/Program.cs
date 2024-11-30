using CoordinatorsAppAPI.AppSettings;
var builder = WebApplication.CreateBuilder(args);

// ----------------------------------
// Konfiguracja plik�w konfiguracyjnych i zmiennych �rodowiskowych
// ----------------------------------
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ----------------------------------
// Rejestracja us�ug
// ----------------------------------

// Pobieranie FrontendUrl z konfiguracji
var frontendUrl = builder.Configuration["AppSettings:FrontendUrl"];

if (string.IsNullOrEmpty(frontendUrl))
{
    throw new InvalidOperationException("FrontendUrl is not configured. Please set it in appsettings.");
}

// Rejestracja us�ug
builder.Services.AddOptions<AppSettings>()
    .Bind(builder.Configuration.GetSection("AppSettings")) // Powi�� z sekcj� w appsettings.json
    .ValidateDataAnnotations() // Walidacja na podstawie atrybut�w [Required], [Url] itd.
    .ValidateOnStart(); // Wymusza walidacj� podczas startu aplikacji


builder.Services.AddOptions<OAuthCredentials>()
    .Bind(builder.Configuration.GetSection("OAuthCredentials"))
    .ValidateDataAnnotations()
    .ValidateOnStart();


// Konfiguracja CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DynamicCorsPolicy", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(frontendUrl) // Zawsze pobiera FrontendUrl z konfiguracji
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Konfiguracja Swagger (opcjonalnie)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dodanie kontroler�w
builder.Services.AddControllers();

var app = builder.Build();

// ----------------------------------
// Konfiguracja aplikacji
// ----------------------------------

// Swagger tylko w �rodowisku deweloperskim
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Obs�uga HTTPS
app.UseHttpsRedirection();

// Ustawienie dynamicznej polityki CORS
app.UseCors("DynamicCorsPolicy");

// Obs�uga autoryzacji
app.UseAuthorization();
app.UseMiddleware<LoggingMiddleware>();
// Mapowanie kontroler�w
app.MapControllers();

// Uruchomienie aplikacji
app.Run();

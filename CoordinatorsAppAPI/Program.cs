using CoordinatorsAppAPI.AppSettings;
var builder = WebApplication.CreateBuilder(args);

// ----------------------------------
// Konfiguracja plików konfiguracyjnych i zmiennych œrodowiskowych
// ----------------------------------
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ----------------------------------
// Rejestracja us³ug
// ----------------------------------

// Pobieranie FrontendUrl z konfiguracji
var frontendUrl = builder.Configuration["AppSettings:FrontendUrl"];

if (string.IsNullOrEmpty(frontendUrl))
{
    throw new InvalidOperationException("FrontendUrl is not configured. Please set it in appsettings.");
}

// Rejestracja us³ug
builder.Services.AddOptions<AppSettings>()
    .Bind(builder.Configuration.GetSection("AppSettings")) // Powi¹¿ z sekcj¹ w appsettings.json
    .ValidateDataAnnotations() // Walidacja na podstawie atrybutów [Required], [Url] itd.
    .ValidateOnStart(); // Wymusza walidacjê podczas startu aplikacji


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

// Dodanie kontrolerów
builder.Services.AddControllers();

var app = builder.Build();

// ----------------------------------
// Konfiguracja aplikacji
// ----------------------------------

// Swagger tylko w œrodowisku deweloperskim
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Obs³uga HTTPS
app.UseHttpsRedirection();

// Ustawienie dynamicznej polityki CORS
app.UseCors("DynamicCorsPolicy");

// Obs³uga autoryzacji
app.UseAuthorization();
app.UseMiddleware<LoggingMiddleware>();
// Mapowanie kontrolerów
app.MapControllers();

// Uruchomienie aplikacji
app.Run();

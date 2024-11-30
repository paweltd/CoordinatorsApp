using System.Text;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Loguj Request Body
        var requestBody = await LogRequestBody(context);

        // Przechwyć oryginalny strumień odpowiedzi
        var originalBodyStream = context.Response.Body;

        using (var responseBody = new MemoryStream())
        {
            // Zamień strumień odpowiedzi na MemoryStream
            context.Response.Body = responseBody;

            try
            {
                // Kontynuuj przetwarzanie kolejnych middleware
                await _next(context);

                // Loguj Response Body
                var responseText = await LogResponseBody(context);
                Console.WriteLine($"Request Body: {requestBody}");
                Console.WriteLine($"Response Body: {responseText}");
            }
            finally
            {
                // Przywróć oryginalny strumień odpowiedzi
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
        }
    }

    private async Task<string> LogRequestBody(HttpContext context)
    {
        context.Request.EnableBuffering(); // Umożliwia wielokrotne odczytanie Request.Body
        context.Request.Body.Position = 0;

        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Reset pozycji, aby inne middleware mogły odczytać Request.Body
            return body;
        }
    }

    private async Task<string> LogResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return text;
    }
}

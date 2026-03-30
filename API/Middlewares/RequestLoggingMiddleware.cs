using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var request = context.Request;

        try
        {
            await _next(context);
            sw.Stop();

            var statusCode = context.Response.StatusCode;
            var elapsedMs = sw.ElapsedMilliseconds;

 
            var logMessage = $"HTTP {request.Method} {request.Path} responded {statusCode} in {elapsedMs}ms";

      
            if (statusCode >= 400)
            {
                _logger.LogWarning("HATA DURUMU: {LogMessage} | Query: {QueryString}",
                    logMessage, request.QueryString);
            }
            else
            {
                _logger.LogInformation(logMessage);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            
            _logger.LogError(ex, "KRİTİK HATA: {Method} {Path} yolunda bir hata oluştu! Süre: {Elapsed}ms",
                request.Method, request.Path, sw.ElapsedMilliseconds);

            throw; 
        }
    }
}
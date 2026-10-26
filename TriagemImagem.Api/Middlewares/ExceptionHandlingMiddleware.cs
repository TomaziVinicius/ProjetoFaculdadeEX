using System.Net;
using System.Text.Json;
using TriagemImagem.Domain.Exceptions;

namespace TriagemImagem.Api.Middlewares;

/// <summary>
/// Middleware global para captura e tratamento padronizado de exceções na API.
/// Converte violações de regras clínicas (DomainException) em HTTP 400 Bad Request
/// e falhas inesperadas em HTTP 500 sem expor detalhes sensíveis.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Regra de negócio violada: {Message} em {Path}", ex.Message, context.Request.Path);
            await TratarExcecaoAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);
            await TratarExcecaoAsync(context, HttpStatusCode.InternalServerError, "Ocorreu um erro interno no processamento do servidor.");
        }
    }

    private static async Task TratarExcecaoAsync(HttpContext context, HttpStatusCode statusCode, string mensagem)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            success = false,
            statusCode = (int)statusCode,
            message = mensagem,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

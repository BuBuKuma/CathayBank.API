using CathayBank.API.Services;
using Microsoft.AspNetCore.Localization;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CathayBank.API.Middlewares
{
    /// <summary>
    /// 合併了日誌記錄與全域例外處理的職責。
    /// </summary>
    public class UnifiedDiagnosticsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UnifiedDiagnosticsMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public UnifiedDiagnosticsMiddleware(
            RequestDelegate next,
            ILogger<UnifiedDiagnosticsMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            var requestBody = await ReadStreamToStringAsync(context.Request.Body);
            context.Request.Body.Position = 0;
            _logger.LogInformation($"外部請求: {context.Request.Method} {context.Request.Path} | Body: {requestBody}");

            var originalBodyStream = context.Response.Body;
            using var responseBodyMemoryStream = new MemoryStream();
            context.Response.Body = responseBodyMemoryStream;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發生未處理的例外: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }

            responseBodyMemoryStream.Position = 0;
            var responseBodyContent = await new StreamReader(responseBodyMemoryStream).ReadToEndAsync();

            _logger.LogInformation($"回覆外部: {context.Response.StatusCode} for {context.Request.Method} {context.Request.Path} | Body: {responseBodyContent}");

            responseBodyMemoryStream.Position = 0;
            await responseBodyMemoryStream.CopyToAsync(originalBodyStream);
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";

            string errorMessage;
            try
            {
                var resourcesPath = Path.Combine(_env.ContentRootPath, "Resources");
                var localizer = new JsonStringLocalizer(resourcesPath, _logger);
                var cultureName = context.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name ?? "en-US";

                errorMessage = localizer.GetString("UnexpectedError", cultureName)
                               ?? "An unexpected error occurred. (JSON lookup failed)";
            }
            catch (Exception resEx)
            {
                _logger.LogError(resEx, "在 JSON 本地化過程中發生例外。");
                errorMessage = "An unexpected error occurred while processing the error message.";
            }

            var errorResponse = new { StatusCode = context.Response.StatusCode, Message = errorMessage };
            var serializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorResponse, serializerOptions));
            await context.Response.Body.WriteAsync(responseBytes);
        }

        private async Task<string> ReadStreamToStringAsync(Stream stream)
        {
            if (!stream.CanRead) return string.Empty;
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var content = await reader.ReadToEndAsync();
            return content;
        }
    }

    public static class UnifiedDiagnosticsMiddlewareExtensions
    {
        public static IApplicationBuilder UseUnifiedDiagnostics(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UnifiedDiagnosticsMiddleware>();
        }
    }
}
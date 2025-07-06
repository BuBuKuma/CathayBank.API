using System.Text;

namespace CathayBank.API.Handlers
{
    /// <summary>
    /// 處理 HttpClient 的紀錄
    /// </summary>
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> _logger;

        public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"發送請求: {request.Method} {request.RequestUri}");

            var response = await base.SendAsync(request, cancellationToken);

            try
            {
                if (response.Content != null)
                {
                    var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                    var responseBodyForLogging = Encoding.UTF8.GetString(responseBytes);

                    if (string.IsNullOrWhiteSpace(responseBodyForLogging))
                    {
                        _logger.LogWarning($"接收成功但 Body 為空. Status: {response.StatusCode}, Uri: {request.RequestUri}");
                    }
                    else
                    {
                        _logger.LogInformation($"接收成功 {request.RequestUri} ({response.StatusCode}) | Body: {responseBodyForLogging}");
                    }

                    response.Content = new ByteArrayContent(responseBytes);
                    if (response.Content.Headers.ContentType == null && response.Content.Headers.TryAddWithoutValidation("Content-Type", "application/json"))
                    {
                        // 確保 Content-Type 存在
                    }
                }
                else
                {
                    _logger.LogWarning($"接收成功但 Content 物件為 null. Status: {response.StatusCode}, Uri: {request.RequestUri}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄外部 API 回應時發生錯誤。");
            }

            return response;
        }
    }
}

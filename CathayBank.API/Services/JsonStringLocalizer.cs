using System.Text.Json;

namespace CathayBank.API.Services
{
    public class JsonStringLocalizer
    {
        private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
        private readonly string _resourcesPath;
        private readonly ILogger _logger;

        public JsonStringLocalizer(string resourcesPath, ILogger logger)
        {
            _resourcesPath = resourcesPath;
            _logger = logger;
        }

        public string? GetString(string key, string culture)
        {
            try
            {
                var filePath = Path.Combine(_resourcesPath, $"SharedResource.{culture}.json");

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("找不到特定文化 JSON 資源檔: {Path}，將使用預設後備訊息。", filePath);
                    return null;
                }

                var jsonContent = File.ReadAllText(filePath);
                var resourceDict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent, _options);

                if (resourceDict != null && resourceDict.TryGetValue(key, out var value))
                {
                    return value;
                }

                _logger.LogWarning("在 JSON 檔案 '{Path}' 中找不到鍵值: '{Key}'", filePath, key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "讀取 JSON 資源檔案時發生錯誤。");
                return null;
            }
        }
    }
}
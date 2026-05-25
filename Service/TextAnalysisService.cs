using Common.DTO;
using Repository.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
     public class TextAnalysisService
     {
        
        private readonly HttpClient _httpClient;
        public TextAnalysisService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
       
        public async Task<List<string>> AnalyzeTextAsync(string textToAnalyze)
        {
            // הכנת האובייקט לשליחה (חייב להתאים ל-content שכתבנו בפייתון)
            var requestBody = new { content = textToAnalyze };
            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/analyze", httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DEBUG: Python returned: {responseJson}");
                    var result = System.Text.Json.JsonSerializer.Deserialize<TextAnalysisResult>(responseJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result?.RelevatWords ?? new List<string>();
                }
            }
            catch (Exception ex)
            {
                throw new AppException($"Python Text Analysis failed: {ex.Message}");
            }

            return new List<string>();
        }

    }
}

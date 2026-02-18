using Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
     public class TextAnalysisService
     {
        // 1. הגדרת ה"טלפון" שדרכו נתקשר עם פייתון
        private readonly HttpClient _httpClient;

        // 2. קונסטרקטור (בנאי) - כאן המערכת מזריקה לנו את ה-HttpClient
        public TextAnalysisService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 3. הפונקציה המרכזית שמקבלת טקסט ומחזירה רשימת מילים (Features)
        public async Task<List<string>> AnalyzeTextAsync(string textToAnalyze)
        {
            // הכנת האובייקט לשליחה (חייב להתאים ל-content שכתבנו בפייתון)
            var requestBody = new { content = textToAnalyze };
            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                // שליחה לשרת הפייתון (הכתובת שראית בטרמינל)
                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/analyze", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"DEBUG: Python returned: {responseJson}");

                    // כאן אנחנו משתמשים ב-DTO (הקופסה) שיצרנו קודם
                    var result = System.Text.Json.JsonSerializer.Deserialize<TextAnalysisResult>(responseJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result?.RelevatWords ?? new List<string>();
                }
            }
            catch (Exception ex)
            {
                // אם הפייתון לא רץ, זה ייפול כאן
                Console.WriteLine($"Error: {ex.Message}");
            }

            return new List<string>(); // במקרה של תקלה מחזירים רשימה ריקה
        }

    }
}

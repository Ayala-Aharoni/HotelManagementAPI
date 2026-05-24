using Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json; // חשוב בשביל PostAsJsonAsync
using System.Text;
using System.Threading.Tasks;
using Service.Interfaces;
using System.Net.Http.Json;

namespace Service
{
    public class SimiliarWordsService : ISimiliarWord
    {
        private readonly HttpClient _httpClient;

        public SimiliarWordsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // הפונקציה האחת והיחידה - מקבלת רשימת מילים במכה אחת ומחזירה מילון
        public async Task<Dictionary<string, List<PythonMatchDTO>>> GetSimilarWordsFromPython(List<string> words, List<string> allKeywords, double threshold)
        {
            Console.WriteLine($"[Service] Asking Python for matches for {words.Count} words...");

            try
            {
                // יצירת האובייקט שיישלח כ-JSON
                var requestBody = new
                {
                    words = words, // שולחים רשימה
                    all_keywords = allKeywords,
                    threshold = threshold
                };

                // שליחת POST לשרת פייתון לכתובת הרגילה שלך!
                var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/find_similar_in_dictionary", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    // קבלת התשובה: מילון שבו המפתח הוא המילה, והערך הוא רשימת ההתאמות שלה
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, List<PythonMatchDTO>>>();

                    if (result != null)
                    {
                        Console.WriteLine($"[Service] Python returned results successfully.");
                        return result;
                    }
                }
                else
                {
                    Console.WriteLine($"[Service] Python Server Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] Communication Exception: {ex.Message}");
            }

            // במקרה של שגיאה נחזיר מילון ריק כדי שהקוד לא יקרוס
            return new Dictionary<string, List<PythonMatchDTO>>();
        }
    }
}

          
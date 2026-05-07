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

        // מימוש הפונקציה החדשה שה-Interface דורש
        public async Task<List<PythonMatchDTO>> GetSimilarWordsFromPython(string word, List<string> allKeywords, double threshold)
        {
            Console.WriteLine($"[Service] Asking Python for matches for: '{word}' among {allKeywords.Count} words...");

            try
            {
                // יצירת האובייקט שיישלח כ-JSON (תואם ל-BaseModel של פייתון)
                var requestBody = new
                {
                    word = word,
                    all_keywords = allKeywords,
                    threshold = threshold
                };

                // שליחת POST לשרת פייתון
                var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8000/find_similar_in_dictionary", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    // קבלת התשובה (המילים הדומות והציון שלהן)
                    var result = await response.Content.ReadFromJsonAsync<PythonResponseDTO>();

                    if (result?.Matches != null)
                    {
                        Console.WriteLine($"[Service] Python returned {result.Matches.Count} matches.");
                        return result.Matches;
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

            return new List<PythonMatchDTO>(); // מחזיר רשימה ריקה במקרה של תקלה
        }

        // הפונקציה הישנה - אפשר להשאיר או למחוק אם ה-Interface כבר לא דורש אותה
        public async Task<List<string>> GetSimilarWordsAsync(string word)
        {
            // ... (הקוד הישן שלך) ...
            return new List<string>();
        }
    }

    // DTO פנימי או חיצוני לקבלת התשובה המלאה מפייתון
    public class PythonResponseDTO
    {
        public List<PythonMatchDTO> Matches { get; set; }
    }
}
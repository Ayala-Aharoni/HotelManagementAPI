using Common.DTO;   
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Service.Interfaces;   
namespace Service
{
    public class SimiliarWordsService : ISimiliarWord
    {
        private readonly HttpClient _httpClient;

        public SimiliarWordsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //public async Task<List<string>> GetSimilarWordsAsync(string word)
        //{
        //    try
        //    {
        //        // קריאת GET לכתובת החדשה בפייתון
        //        var response = await _httpClient.GetAsync($"http://127.0.0.1:8000/synonyms/{word}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var responseJson = await response.Content.ReadAsStringAsync();

        //            var result = System.Text.Json.JsonSerializer.Deserialize<SynonymResponseDTO>(responseJson,
        //                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //            return result?.Synonyms ?? new List<string>();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[SimilarWordsService] Error: {ex.Message}");
        //    }

        //    return new List<string>();
        //}
        public async Task<List<string>> GetSimilarWordsAsync(string word)
        {
            Console.WriteLine($"[Service] נסה להביא מילים דומות עבור: '{word}'...");

            try
            {
                var response = await _httpClient.GetAsync($"http://127.0.0.1:8000/synonyms/{word}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    // הדפסה של ה-JSON הגולמי שחזר מפייתון
                    Console.WriteLine($"[Service] הצלחה! התשובה מפייתון: {responseJson}");

                    var result = System.Text.Json.JsonSerializer.Deserialize<SynonymResponseDTO>(responseJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result?.Synonyms ?? new List<string>();
                }
                else
                {
                    Console.WriteLine($"[Service] שגיאה מהשרת: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Service] תקלה בתקשורת: {ex.Message}");
            }

            return new List<string>();
        }
    }

   
}


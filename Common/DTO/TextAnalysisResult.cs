using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization; // חובה להוסיף את ה-using הזה!
using System.Threading.Tasks;

namespace Common.DTO
{
    public class TextAnalysisResult
    {
        [JsonPropertyName("features")]//זה בשביל המרת גיסון
        public List<string> RelevatWords { get; set; }
    }
}
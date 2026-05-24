using Common.DTO;
using Repository.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Service.Interfaces
{
    public interface INaiveBase
    {
        // הפונקציות הראשיות שכולם צריכים
        Task LoadModel();
        Task<int> PredictCategory(List<string> words);

        // למידה בזמן אמת - אם את רוצה לעדכן מילה בודדת בלי לטעון הכל מחדש
        void AddNewWordToDictinary(string wordText, int categoryId, int wordId);

        // חשיפת המילון לקריאה בלבד (אם צריך להציג סטטיסטיקות)
        ConcurrentDictionary<string, WordClassificationDTO> WordStatistics { get; }
   //     Dictionary<string, WordClassificationDTO> WordStatistics { get; }

        // פונקציית עזר לתרגום ID לאינדקס (אם היא בשימוש מחוץ לקלאס)
        int GetIndex(int categoryId);
    }
}
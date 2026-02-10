using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class WordClassificationDTO
    {
        public int WordId { get; set; } 
        public string Word { get; set; }
      
        public int[] CategoryCounts { get; set; }

        public WordClassificationDTO(int categoryCount)
        {
            CategoryCounts = new int[categoryCount];
        }

    }
}

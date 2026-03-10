using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class PredictionResultDTO
    {
        public int CategoryId { get; set; }
      
        public List<string> WordsToLearn { get; set; } = new List<string>();
    }
}

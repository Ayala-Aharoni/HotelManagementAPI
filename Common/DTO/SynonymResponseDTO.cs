using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class SynonymResponseDTO
    {
        public string Word { get; set; }
        public List<string> Synonyms { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class Word
    {
        public int WordId { get; set; }
        public string Text { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        public int Frequency { get; set; }
        public ICollection<CategoryWord> CategoryWords { get; set; }
      = new List<CategoryWord>();
    }
}




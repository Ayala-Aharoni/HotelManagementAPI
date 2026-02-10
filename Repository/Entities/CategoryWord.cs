using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class CategoryWord
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public int WordId { get; set; }
        [ForeignKey("WordId")]
        public Word Word { get; set; }
        public int Frequency { get; set; }

    }

}

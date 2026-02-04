using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.DTO
{
    public class WordDTO
    {
        [Required(ErrorMessage = "המילה חובה")]
        public string Text { get; set; }

        [Required(ErrorMessage = "קטגוריה חובה")]
        public int CategoryId { get; set; }

        [Range(1, 100, ErrorMessage = "Frequency חייב להיות בין 1 ל-100")]
        public int Frequency { get; set; }
    }
}

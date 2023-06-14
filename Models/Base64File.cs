using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookHaven.Models
{
    public class Base64File
    {
        public Guid Id { get; set; }

        public string FileName { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Data { get; set; }

        public Guid BookId { get; set; }

        public Book Book { get; set; }
    }
}

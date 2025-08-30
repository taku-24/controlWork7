using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlWork7.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = null!;

        [Required, StringLength(200)]
        public string Author { get; set; } = null!;
        
        [Required, Url, StringLength(500)]
        public string CoverPath { get; set; } = null!;

        [Range(1400, 2100)]
        public int? Year { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public Category? Category { get; set; } = null!;

        
        [Required(ErrorMessage = "Выберите категорию.")]
        public int CategoryId { get; set; }

        public ICollection<Issue> Issues { get; set; } = new List<Issue>();

        [NotMapped]
        public bool IsAvailable => !Issues.Any(i => i.ReturnedAt == null);
    }
}
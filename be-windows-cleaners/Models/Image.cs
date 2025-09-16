using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace be_windows_cleaners.Models
{
    public class Image
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string ImageData { get; set; } = string.Empty; // Base64 string
        
        // Foreign key to User
        [Required]
        public int UserId { get; set; }
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

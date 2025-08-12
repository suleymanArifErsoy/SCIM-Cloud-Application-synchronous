using System.ComponentModel.DataAnnotations;

namespace SCIM_Application.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string? ExternalId { get; set; }
        public string? ScimId { get; set; }

        public virtual ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();
    }
}

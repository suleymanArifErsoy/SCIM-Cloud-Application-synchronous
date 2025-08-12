using System.ComponentModel.DataAnnotations;

namespace SCIM_Application.Models
{
    public class Application
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string ScimEndpoint { get; set; } = string.Empty;

        [StringLength(255)]
        public string ApiKey { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? BearerToken { get; set; }

        [Required, StringLength(50)]
        public string Provider { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();
    }
}

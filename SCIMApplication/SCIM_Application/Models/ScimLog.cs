using System.ComponentModel.DataAnnotations;

namespace SCIM_Application.Models
{
    public class ScimLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? ApplicationId { get; set; }

        [Required, StringLength(50)]
        public string Operation { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Status { get; set; } = string.Empty;

        public string? RequestData { get; set; }
        public string? ResponseData { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? ResponseTimeMs { get; set; }

        public virtual User? User { get; set; }
        public virtual Application? Application { get; set; }
    }
}

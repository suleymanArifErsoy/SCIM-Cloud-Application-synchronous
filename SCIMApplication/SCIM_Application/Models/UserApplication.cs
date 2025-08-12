namespace SCIM_Application.Models
{
    public class UserApplication
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ApplicationId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? ScimStatus { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public string? LastSyncError { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Application Application { get; set; } = null!;
    }
}

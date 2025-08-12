using System.ComponentModel.DataAnnotations;

namespace SCIM_Application.ViewModels
{
    public class UserFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur"), StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad zorunludur"), StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur"), StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur"), EmailAddress, StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public List<int> SelectedApplicationIds { get; set; } = new();
        public List<SelectableApplication> AvailableApplications { get; set; } = new();
    }

    public class SelectableApplication
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}

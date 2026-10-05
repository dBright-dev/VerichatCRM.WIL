using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Web.Models.Client
{
    public class CreateClientViewModel
    {
        [Required, MaxLength(200)]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = "";

        [Required, MaxLength(100)]
        [Display(Name = "Sector")]
        public string Sector { get; set; } = "";

        [MaxLength(150)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = "";

        [EmailAddress, MaxLength(200)]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = "";

        [MaxLength(30)]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = "";

        [MaxLength(500)]
        public string Address { get; set; } = "";
    }
}

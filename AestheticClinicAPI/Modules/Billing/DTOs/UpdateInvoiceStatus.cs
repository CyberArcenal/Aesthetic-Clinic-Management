using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Billing.DTOs
{
    public class UpdateInvoiceStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Draft, Sent, Paid, Partial, Overdue, Cancelled
    }
}
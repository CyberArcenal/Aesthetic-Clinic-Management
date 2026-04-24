using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Billing.DTOs
{
    public class CreateInvoiceDto
    {
        [Required]
        public int ClientId { get; set; }

        public int? AppointmentId { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public decimal Subtotal { get; set; }

        public decimal Tax { get; set; } = 0;

        public string? Notes { get; set; }
    }
}
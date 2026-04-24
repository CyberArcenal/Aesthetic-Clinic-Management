using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Billing.DTOs
{
    public class CreatePaymentDto
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public string Method { get; set; } = "Cash";

        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}
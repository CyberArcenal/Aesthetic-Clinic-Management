using AestheticClinicAPI.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Billing.Models
{
    public class Payment : BaseEntity
    {
        [Required]
        public int InvoiceId { get; set; }

        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required, MaxLength(50)]
        public string Method { get; set; } = "Cash"; // Cash, CreditCard, DebitCard, GCash, BankTransfer

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        public string? Notes { get; set; }
    }
}
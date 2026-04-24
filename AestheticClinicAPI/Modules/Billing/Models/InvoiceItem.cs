using AestheticClinicAPI.Modules.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Billing.Models
{
    public class InvoiceItem : BaseEntity
    {
        [Required]
        public int InvoiceId { get; set; }

        public int? TreatmentId { get; set; }

        [Required, MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }

        public decimal Total => Quantity * UnitPrice;
    }
}
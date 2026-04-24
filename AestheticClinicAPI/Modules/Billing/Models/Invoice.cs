using AestheticClinicAPI.Modules.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Billing.Models
{
    public class Invoice : BaseEntity
    {
        [Required]
        public int ClientId { get; set; }

        public int? AppointmentId { get; set; }  // optional, links to an appointment

        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Tax { get; set; } = 0;

        public decimal Total { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Partial, Overdue, Cancelled

        public string? Notes { get; set; }
    }
}
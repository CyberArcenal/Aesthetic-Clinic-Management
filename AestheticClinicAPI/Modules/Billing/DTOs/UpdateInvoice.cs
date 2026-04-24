namespace AestheticClinicAPI.Modules.Billing.DTOs
{
    public class UpdateInvoiceDto
    {
        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? Tax { get; set; }
        public string? Notes { get; set; }
    }
}
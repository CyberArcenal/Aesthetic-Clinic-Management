using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Billing.Models;

namespace AestheticClinicAPI.Modules.Billing.Repositories
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<IEnumerable<Invoice>> GetByClientAsync(int clientId);
        Task<IEnumerable<Invoice>> GetByStatusAsync(string status);
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<decimal> GetTotalPaidByInvoiceAsync(int invoiceId);
    }
}
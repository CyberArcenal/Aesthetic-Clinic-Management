using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Billing.Models;

namespace AestheticClinicAPI.Modules.Billing.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByInvoiceAsync(int invoiceId);
        Task<IEnumerable<Payment>> GetByMethodAsync(string method);
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<decimal> GetTotalPaymentsByDateRangeAsync(DateTime start, DateTime end);
    }
}
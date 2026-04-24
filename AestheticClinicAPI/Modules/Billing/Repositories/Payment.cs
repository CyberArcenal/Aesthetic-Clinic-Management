using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Billing.Models;

namespace AestheticClinicAPI.Modules.Billing.Repositories
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetByInvoiceAsync(int invoiceId)
        {
            return await _dbSet
                .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByMethodAsync(string method)
        {
            return await _dbSet
                .Where(p => p.Method == method && !p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= start && p.PaymentDate <= end && !p.IsDeleted)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= start && p.PaymentDate <= end && !p.IsDeleted)
                .SumAsync(p => p.Amount);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Billing.Models;

namespace AestheticClinicAPI.Modules.Billing.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Invoice>> GetByClientAsync(int clientId)
        {
            return await _dbSet
                .Where(i => i.ClientId == clientId && !i.IsDeleted)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(i => i.Status == status && !i.IsDeleted)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber && !i.IsDeleted);
        }

        public async Task<decimal> GetTotalPaidByInvoiceAsync(int invoiceId)
        {
            var total = await _context.Set<Payment>()
                .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
                .SumAsync(p => p.Amount);
            return total;
        }
    }
}
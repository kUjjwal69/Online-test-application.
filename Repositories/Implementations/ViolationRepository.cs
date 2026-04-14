using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Data;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;
using TestManagementApplication.Repositories.Interfaces;

namespace TestManagementApplication.Repositories.Implementations
{
    public class ViolationRepository : Repository<Violation>, IViolationRepository
    {
        public ViolationRepository(AppDbContext context) : base(context)
        {
        }

    

        public async Task<IEnumerable<Violation>> GetBySessionIdAsync(Guid sessionId)
            => await _context.Violations
                .Where(v => v.SessionId == sessionId)
                .OrderByDescending(v => v.OccurredAt)
                .ToListAsync();

        public async Task<Violation> CreateAsync(Violation violation)
        {
            _context.Violations.Add(violation);
            await _context.SaveChangesAsync();
            return violation;
        }

        public async Task<int> CountBySessionIdAsync(Guid sessionId)
            => await _context.Violations.CountAsync(v => v.SessionId == sessionId);
    }

    public class CapturedImageRepository : Repository<CapturedImage>, ICapturedImageRepository
    {
        public CapturedImageRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CapturedImage>> GetBySessionIdAsync(Guid sessionId)
            => await _context.CapturedImages
                .Where(c => c.SessionId == sessionId)
                .OrderByDescending(c => c.CapturedAt)
                .ToListAsync();

        public async Task<CapturedImage> CreateAsync(CapturedImage image)
        {
            _context.CapturedImages.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }
    }

}

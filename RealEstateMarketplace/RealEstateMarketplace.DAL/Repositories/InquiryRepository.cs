using Microsoft.EntityFrameworkCore;
using RealEstateMarketplace.DAL.Data;
using RealEstateMarketplace.DAL.Entities;

namespace RealEstateMarketplace.DAL.Repositories;

public class InquiryRepository : Repository<Inquiry>, IInquiryRepository
{
    private readonly ApplicationDbContext _context;

    public InquiryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Inquiry>> GetInquiriesWithDetailsAsync()
    {
        return await _context.Inquiries
            .Include(i => i.Sender)
            .Include(i => i.Receiver)
            .Include(i => i.Property)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Inquiry?> GetInquiryWithDetailsAsync(int id)
    {
        return await _context.Inquiries
            .Include(i => i.Sender)
            .Include(i => i.Receiver)
            .Include(i => i.Property)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Inquiry>> GetInquiriesByPropertyAsync(int propertyId)
    {
        return await _context.Inquiries
            .Include(i => i.Sender)
            .Include(i => i.Receiver)
            .Where(i => i.PropertyId == propertyId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inquiry>> GetInquiriesByReceiverAsync(string receiverId)
    {
        return await _context.Inquiries
            .Include(i => i.Sender)
            .Include(i => i.Property)
            .Where(i => i.ReceiverId == receiverId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inquiry>> GetInquiriesBySenderAsync(string senderId)
    {
        return await _context.Inquiries
            .Include(i => i.Receiver)
            .Include(i => i.Property)
            .Where(i => i.SenderId == senderId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inquiry>> GetUnreadInquiriesAsync(string receiverId)
    {
        return await _context.Inquiries
            .Include(i => i.Sender)
            .Include(i => i.Property)
            .Where(i => i.ReceiverId == receiverId && !i.IsRead)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string receiverId)
    {
        return await _context.Inquiries
            .CountAsync(i => i.ReceiverId == receiverId && !i.IsRead);
    }
}

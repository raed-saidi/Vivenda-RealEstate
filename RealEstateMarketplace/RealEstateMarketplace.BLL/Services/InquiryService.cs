using Microsoft.EntityFrameworkCore;
using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.DAL.Data;
using RealEstateMarketplace.DAL.Entities;
using RealEstateMarketplace.DAL.Repositories;

namespace RealEstateMarketplace.BLL.Services;

public class InquiryService : IInquiryService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    
    public InquiryService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IEnumerable<InquiryDto>> GetAllInquiriesAsync()
    {
        var inquiries = await _context.Inquiries
            .Include(i => i.Property)
            .Include(i => i.Sender)
            .Include(i => i.Receiver)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
        
        return inquiries.Select(MapToDto);
    }
    
    public async Task<InquiryDto?> GetInquiryByIdAsync(int id)
    {
        var inquiry = await _context.Inquiries
            .Include(i => i.Property)
            .Include(i => i.Sender)
            .Include(i => i.Receiver)
            .FirstOrDefaultAsync(i => i.Id == id);
        
        return inquiry == null ? null : MapToDto(inquiry);
    }
    
    public async Task<IEnumerable<InquiryDto>> GetInquiriesByPropertyAsync(int propertyId)
    {
        var inquiries = await _context.Inquiries
            .Include(i => i.Property)
            .Include(i => i.Sender)
            .Where(i => i.PropertyId == propertyId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
        
        return inquiries.Select(MapToDto);
    }
    
    public async Task<IEnumerable<InquiryDto>> GetInquiriesByReceiverAsync(string receiverId)
    {
        var inquiries = await _context.Inquiries
            .Include(i => i.Property)
            .Include(i => i.Sender)
            .Where(i => i.ReceiverId == receiverId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
        
        return inquiries.Select(MapToDto);
    }
    
    public async Task<IEnumerable<InquiryDto>> GetUnreadInquiriesAsync()
    {
        var inquiries = await _context.Inquiries
            .Include(i => i.Property)
            .Include(i => i.Sender)
            .Where(i => !i.IsRead)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
        
        return inquiries.Select(MapToDto);
    }
    
    public async Task<int> GetUnreadCountAsync()
    {
        return await _context.Inquiries.CountAsync(i => !i.IsRead);
    }
    
    public async Task<bool> CreateInquiryAsync(CreateInquiryDto dto, string? senderId, string receiverId)
    {
        var inquiry = new Inquiry
        {
            Subject = dto.Subject,
            Message = dto.Message,
            SenderName = dto.SenderName,
            SenderEmail = dto.SenderEmail,
            SenderPhone = dto.SenderPhone,
            PropertyId = dto.PropertyId,
            SenderId = senderId,
            ReceiverId = receiverId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Inquiries.Add(inquiry);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> MarkAsReadAsync(int id)
    {
        var inquiry = await _context.Inquiries.FindAsync(id);
        if (inquiry == null) return false;
        
        inquiry.IsRead = true;
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> MarkAsUnreadAsync(int id)
    {
        var inquiry = await _context.Inquiries.FindAsync(id);
        if (inquiry == null) return false;
        
        inquiry.IsRead = false;
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteInquiryAsync(int id)
    {
        var inquiry = await _context.Inquiries.FindAsync(id);
        if (inquiry == null) return false;
        
        _context.Inquiries.Remove(inquiry);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> ReplyToInquiryAsync(int inquiryId, string message, string senderId)
    {
        var originalInquiry = await _context.Inquiries.FindAsync(inquiryId);
        if (originalInquiry == null) return false;
        
        // Create reply as a new inquiry
        var reply = new Inquiry
        {
            Subject = $"RE: {originalInquiry.Subject}",
            Message = message,
            PropertyId = originalInquiry.PropertyId,
            SenderId = senderId,
            ReceiverId = originalInquiry.SenderId ?? originalInquiry.ReceiverId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        
        // Mark original as read
        originalInquiry.IsRead = true;
        
        _context.Inquiries.Add(reply);
        return await _context.SaveChangesAsync() > 0;
    }
    
    private static InquiryDto MapToDto(Inquiry inquiry)
    {
        return new InquiryDto
        {
            Id = inquiry.Id,
            Subject = inquiry.Subject,
            Message = inquiry.Message,
            SenderName = inquiry.SenderName ?? (inquiry.Sender != null ? $"{inquiry.Sender.FirstName} {inquiry.Sender.LastName}" : null),
            SenderEmail = inquiry.SenderEmail ?? inquiry.Sender?.Email,
            SenderPhone = inquiry.SenderPhone ?? inquiry.Sender?.PhoneNumber,
            IsRead = inquiry.IsRead,
            CreatedAt = inquiry.CreatedAt,
            PropertyId = inquiry.PropertyId,
            PropertyTitle = inquiry.Property?.Title ?? "Unknown"
        };
    }
}

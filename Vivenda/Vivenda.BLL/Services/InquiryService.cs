using Microsoft.EntityFrameworkCore;
using Vivenda.BLL.DTOs;
using Vivenda.DAL.Data;
using Vivenda.DAL.Entities;
using Vivenda.DAL.Repositories;

namespace Vivenda.BLL.Services;

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

    public async Task<IEnumerable<InquiryDto>> GetInquiriesBySenderAsync(string senderId)
    {
        var inquiries = await _context.Inquiries
            .Include(i => i.Property)
            .Include(i => i.Receiver)
            .Where(i => i.SenderId == senderId)
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

    public async Task<int> GetUnreadCountByReceiverAsync(string receiverId)
    {
        return await _context.Inquiries.CountAsync(i => i.ReceiverId == receiverId && !i.IsRead);
    }

    public async Task<InquiryDto> CreateInquiryAsync(CreateInquiryDto dto)
    {
        var inquiry = new Inquiry
        {
            Subject = dto.Subject,
            Message = dto.Message,
            SenderName = dto.SenderName,
            SenderEmail = dto.SenderEmail,
            SenderPhone = dto.SenderPhone,
            PropertyId = dto.PropertyId,
            SenderId = dto.SenderId,
            ReceiverId = dto.ReceiverId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Inquiries.Add(inquiry);
        await _context.SaveChangesAsync();

        return await GetInquiryByIdAsync(inquiry.Id) ?? MapToDto(inquiry);
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
            SenderId = inquiry.SenderId,
            ReceiverId = inquiry.ReceiverId,
            ReceiverName = inquiry.Receiver != null ? $"{inquiry.Receiver.FirstName} {inquiry.Receiver.LastName}" : "",
            ReceiverEmail = inquiry.Receiver?.Email,
            IsRead = inquiry.IsRead,
            CreatedAt = inquiry.CreatedAt,
            PropertyId = inquiry.PropertyId,
            PropertyTitle = inquiry.Property?.Title ?? "Unknown"
        };
    }
}


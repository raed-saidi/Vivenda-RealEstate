using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.BLL.Services;

public interface IInquiryService
{
    Task<IEnumerable<InquiryDto>> GetAllInquiriesAsync();
    Task<InquiryDto?> GetInquiryByIdAsync(int id);
    Task<IEnumerable<InquiryDto>> GetInquiriesByPropertyAsync(int propertyId);
    Task<IEnumerable<InquiryDto>> GetInquiriesByReceiverAsync(string receiverId);
    Task<IEnumerable<InquiryDto>> GetUnreadInquiriesAsync();
    Task<int> GetUnreadCountAsync();
    Task<bool> CreateInquiryAsync(CreateInquiryDto dto, string? senderId, string receiverId);
    Task<bool> MarkAsReadAsync(int id);
    Task<bool> MarkAsUnreadAsync(int id);
    Task<bool> DeleteInquiryAsync(int id);
    Task<bool> ReplyToInquiryAsync(int inquiryId, string message, string senderId);
}

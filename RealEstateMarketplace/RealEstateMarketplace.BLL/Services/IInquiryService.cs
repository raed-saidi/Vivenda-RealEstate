using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.BLL.Services;

public interface IInquiryService
{
    Task<IEnumerable<InquiryDto>> GetAllInquiriesAsync();
    Task<InquiryDto?> GetInquiryByIdAsync(int id);
    Task<IEnumerable<InquiryDto>> GetInquiriesByPropertyAsync(int propertyId);
    Task<IEnumerable<InquiryDto>> GetInquiriesByReceiverAsync(string receiverId);
    Task<IEnumerable<InquiryDto>> GetInquiriesBySenderAsync(string senderId);
    Task<IEnumerable<InquiryDto>> GetUnreadInquiriesAsync();
    Task<int> GetUnreadCountAsync();
    Task<int> GetUnreadCountByReceiverAsync(string receiverId);
    Task<InquiryDto> CreateInquiryAsync(CreateInquiryDto dto);
    Task<bool> MarkAsReadAsync(int id);
    Task<bool> MarkAsUnreadAsync(int id);
    Task<bool> DeleteInquiryAsync(int id);
}

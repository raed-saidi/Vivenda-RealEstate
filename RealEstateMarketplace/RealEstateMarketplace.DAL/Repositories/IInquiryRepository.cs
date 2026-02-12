using RealEstateMarketplace.DAL.Entities;

namespace RealEstateMarketplace.DAL.Repositories;

public interface IInquiryRepository : IRepository<Inquiry>
{
    Task<IEnumerable<Inquiry>> GetInquiriesWithDetailsAsync();
    Task<Inquiry?> GetInquiryWithDetailsAsync(int id);
    Task<IEnumerable<Inquiry>> GetInquiriesByPropertyAsync(int propertyId);
    Task<IEnumerable<Inquiry>> GetInquiriesByReceiverAsync(string receiverId);
    Task<IEnumerable<Inquiry>> GetInquiriesBySenderAsync(string senderId);
    Task<IEnumerable<Inquiry>> GetUnreadInquiriesAsync(string receiverId);
    Task<int> GetUnreadCountAsync(string receiverId);
}

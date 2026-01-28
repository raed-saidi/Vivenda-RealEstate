namespace RealEstateMarketplace.DAL.Repositories;

public interface IUnitOfWork : IDisposable
{
    IPropertyRepository Properties { get; }
    IInquiryRepository Inquiries { get; }
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
}

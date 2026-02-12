using RealEstateMarketplace.DAL.Data;

namespace RealEstateMarketplace.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IPropertyRepository? _propertyRepository;
    private IInquiryRepository? _inquiryRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IPropertyRepository Properties => _propertyRepository ??= new PropertyRepository(_context);
    public IInquiryRepository Inquiries => _inquiryRepository ??= new InquiryRepository(_context);

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            var repository = new Repository<T>(_context);
            _repositories.Add(type, repository);
        }

        return (IRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vivenda.BLL.DTOs;
using Vivenda.DAL.Data;
using Vivenda.DAL.Entities;
using Vivenda.DAL.Repositories;

namespace Vivenda.BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    
    public DashboardService(IUnitOfWork unitOfWork, UserManager<User> userManager, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _context = context;
    }
    
    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        // Property stats
        var totalProperties = await _unitOfWork.Properties.CountAsync();
        var activeProperties = await _unitOfWork.Properties.CountAsync(p => p.Status == PropertyStatus.Active);
        var pendingProperties = await _unitOfWork.Properties.CountAsync(p => p.Status == PropertyStatus.Pending);
        var soldProperties = await _unitOfWork.Properties.CountAsync(p => p.Status == PropertyStatus.Sold);
        var propertiesForSale = await _unitOfWork.Properties.CountAsync(p => p.ListingType == ListingType.Sale && p.Status == PropertyStatus.Active);
        var propertiesForRent = await _unitOfWork.Properties.CountAsync(p => p.ListingType == ListingType.Rent && p.Status == PropertyStatus.Active);
        
        // User stats
        var totalUsers = await _userManager.Users.CountAsync();
        var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
        
        // Inquiry stats
        var inquiryRepo = _unitOfWork.Repository<Inquiry>();
        var totalInquiries = await inquiryRepo.CountAsync();
        var unreadInquiries = await inquiryRepo.CountAsync(i => !i.IsRead);
        
        // Other stats
        var totalCategories = await _unitOfWork.Repository<Category>().CountAsync();
        var totalFavorites = await _unitOfWork.Repository<Favorite>().CountAsync();
        
        // Recent properties
        var recentProperties = await _unitOfWork.Properties.GetLatestPropertiesAsync(5);
        
        // Recent inquiries
        var recentInquiries = await inquiryRepo.FindAsync(i => true);
        
        // Category stats for pie chart
        var categoryStats = await GetCategoryStatsAsync();
        
        // Monthly stats for line chart
        var monthlyStats = await GetMonthlyStatsAsync();
        
        return new DashboardStatsDto
        {
            TotalProperties = totalProperties,
            ActiveProperties = activeProperties,
            PendingProperties = pendingProperties,
            SoldProperties = soldProperties,
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalInquiries = totalInquiries,
            UnreadInquiries = unreadInquiries,
            PropertiesForSale = propertiesForSale,
            PropertiesForRent = propertiesForRent,
            TotalCategories = totalCategories,
            TotalFavorites = totalFavorites,
            RecentProperties = recentProperties.Select(p => new PropertyDto
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                City = p.City,
                Status = p.Status.ToString(),
                MainImageUrl = p.MainImageUrl,
                CreatedAt = p.CreatedAt
            }).ToList(),
            RecentInquiries = recentInquiries.OrderByDescending(i => i.CreatedAt).Take(5).Select(i => new InquiryDto
            {
                Id = i.Id,
                Subject = i.Subject,
                SenderName = i.SenderName,
                SenderEmail = i.SenderEmail,
                IsRead = i.IsRead,
                CreatedAt = i.CreatedAt
            }).ToList(),
            CategoryStats = categoryStats,
            MonthlyStats = monthlyStats
        };
    }
    
    private async Task<List<CategoryStatsDto>> GetCategoryStatsAsync()
    {
        var colors = new[] { "#007bff", "#28a745", "#ffc107", "#dc3545", "#17a2b8", "#6f42c1" };
        var categories = await _context.Categories
            .Include(c => c.Properties)
            .Where(c => c.IsActive)
            .ToListAsync();
        
        return categories.Select((c, index) => new CategoryStatsDto
        {
            CategoryName = c.Name,
            PropertyCount = c.Properties?.Count ?? 0,
            Color = colors[index % colors.Length]
        }).ToList();
    }
    
    private async Task<List<MonthlyStatsDto>> GetMonthlyStatsAsync()
    {
        var stats = new List<MonthlyStatsDto>();
        var today = DateTime.UtcNow;
        
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            
            var propertiesAdded = await _context.Properties
                .CountAsync(p => p.CreatedAt >= monthStart && p.CreatedAt < monthEnd);
            
            var inquiriesReceived = await _context.Inquiries
                .CountAsync(inq => inq.CreatedAt >= monthStart && inq.CreatedAt < monthEnd);
            
            var usersRegistered = await _userManager.Users
                .CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);
            
            stats.Add(new MonthlyStatsDto
            {
                Month = monthStart.ToString("MMM yyyy"),
                PropertiesAdded = propertiesAdded,
                InquiriesReceived = inquiriesReceived,
                UsersRegistered = usersRegistered
            });
        }
        
        return stats;
    }
}


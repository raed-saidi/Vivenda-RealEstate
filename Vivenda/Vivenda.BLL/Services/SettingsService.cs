using Microsoft.EntityFrameworkCore;
using Vivenda.BLL.DTOs;
using Vivenda.DAL.Data;
using Vivenda.DAL.Entities;

namespace Vivenda.BLL.Services;

public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _context;
    
    public SettingsService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<SettingsDto>> GetAllSettingsAsync()
    {
        var settings = await _context.SiteSettings
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .ToListAsync();
        
        return settings.Select(MapToDto);
    }
    
    public async Task<IEnumerable<SettingsDto>> GetSettingsByCategoryAsync(string category)
    {
        var settings = await _context.SiteSettings
            .Where(s => s.Category == category)
            .OrderBy(s => s.Key)
            .ToListAsync();
        
        return settings.Select(MapToDto);
    }
    
    public async Task<string?> GetSettingValueAsync(string key)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }
    
    public async Task<bool> UpdateSettingAsync(string key, string value)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return false;
        
        setting.Value = value;
        setting.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> CreateSettingAsync(SettingsDto dto)
    {
        var setting = new SiteSettings
        {
            Key = dto.Key,
            Value = dto.Value,
            Description = dto.Description,
            Category = dto.Category,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.SiteSettings.Add(setting);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteSettingAsync(string key)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return false;
        
        _context.SiteSettings.Remove(setting);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<Dictionary<string, string>> GetSettingsDictionaryAsync()
    {
        var settings = await _context.SiteSettings.ToListAsync();
        return settings.ToDictionary(s => s.Key, s => s.Value);
    }
    
    private static SettingsDto MapToDto(SiteSettings setting)
    {
        return new SettingsDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            Category = setting.Category,
            UpdatedAt = setting.UpdatedAt
        };
    }
}


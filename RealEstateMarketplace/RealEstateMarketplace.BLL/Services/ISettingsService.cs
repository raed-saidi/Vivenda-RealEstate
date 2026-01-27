using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.BLL.Services;

public interface ISettingsService
{
    Task<IEnumerable<SettingsDto>> GetAllSettingsAsync();
    Task<IEnumerable<SettingsDto>> GetSettingsByCategoryAsync(string category);
    Task<string?> GetSettingValueAsync(string key);
    Task<bool> UpdateSettingAsync(string key, string value);
    Task<bool> CreateSettingAsync(SettingsDto dto);
    Task<bool> DeleteSettingAsync(string key);
    Task<Dictionary<string, string>> GetSettingsDictionaryAsync();
}

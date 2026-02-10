using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vivenda.BLL.DTOs;
using Vivenda.BLL.Services;

namespace Vivenda.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    
    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }
    
    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetAllSettingsAsync();
        var groupedSettings = settings.GroupBy(s => s.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
        return View(groupedSettings);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        return View(new SettingsDto());
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SettingsDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }
        
        var result = await _settingsService.CreateSettingAsync(dto);
        if (result)
        {
            TempData["Success"] = "Setting created successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        ModelState.AddModelError("", "Failed to create setting.");
        return View(dto);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string key, string value)
    {
        var result = await _settingsService.UpdateSettingAsync(key, value);
        if (result)
        {
            TempData["Success"] = "Setting updated successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to update setting.";
        }
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(string key)
    {
        var result = await _settingsService.DeleteSettingAsync(key);
        if (result)
        {
            TempData["Success"] = "Setting deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to delete setting.";
        }
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAll(Dictionary<string, string> settings)
    {
        foreach (var setting in settings)
        {
            await _settingsService.UpdateSettingAsync(setting.Key, setting.Value);
        }
        TempData["Success"] = "All settings saved successfully.";
        return RedirectToAction(nameof(Index));
    }
}


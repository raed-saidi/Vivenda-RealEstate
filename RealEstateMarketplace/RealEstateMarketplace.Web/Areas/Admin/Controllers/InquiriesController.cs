using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateMarketplace.BLL.Services;

namespace RealEstateMarketplace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class InquiriesController : Controller
{
    private readonly IInquiryService _inquiryService;
    private readonly IPropertyService _propertyService;
    
    public InquiriesController(IInquiryService inquiryService, IPropertyService propertyService)
    {
        _inquiryService = inquiryService;
        _propertyService = propertyService;
    }
    
    public async Task<IActionResult> Index(string? filter)
    {
        var inquiries = filter switch
        {
            "unread" => await _inquiryService.GetUnreadInquiriesAsync(),
            "read" => (await _inquiryService.GetAllInquiriesAsync()).Where(i => i.IsRead),
            _ => await _inquiryService.GetAllInquiriesAsync()
        };
        
        ViewBag.Filter = filter;
        ViewBag.UnreadCount = await _inquiryService.GetUnreadCountAsync();
        return View(inquiries);
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var inquiry = await _inquiryService.GetInquiryByIdAsync(id);
        if (inquiry == null)
        {
            return NotFound();
        }
        
        // Mark as read when viewing
        await _inquiryService.MarkAsReadAsync(id);
        inquiry.IsRead = true;
        
        return View(inquiry);
    }
    
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _inquiryService.MarkAsReadAsync(id);
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> MarkAsUnread(int id)
    {
        await _inquiryService.MarkAsUnreadAsync(id);
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _inquiryService.DeleteInquiryAsync(id);
        TempData["Success"] = "Message deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> BulkMarkAsRead(int[] ids)
    {
        foreach (var id in ids)
        {
            await _inquiryService.MarkAsReadAsync(id);
        }
        TempData["Success"] = $"{ids.Length} messages marked as read.";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> BulkDelete(int[] ids)
    {
        foreach (var id in ids)
        {
            await _inquiryService.DeleteInquiryAsync(id);
        }
        TempData["Success"] = $"{ids.Length} messages deleted.";
        return RedirectToAction(nameof(Index));
    }
}

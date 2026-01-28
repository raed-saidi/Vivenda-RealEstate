using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateMarketplace.BLL.Services;
using RealEstateMarketplace.DAL.Entities;

namespace RealEstateMarketplace.Web.Controllers;

[Authorize]
public class InquiriesController : Controller
{
    private readonly IInquiryService _inquiryService;
    private readonly UserManager<User> _userManager;

    public InquiriesController(IInquiryService inquiryService, UserManager<User> userManager)
    {
        _inquiryService = inquiryService;
        _userManager = userManager;
    }

    // My sent inquiries
    public async Task<IActionResult> Sent()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var inquiries = await _inquiryService.GetInquiriesBySenderAsync(user.Id);
        return View(inquiries);
    }

    // My received inquiries (for agents who own properties)
    public async Task<IActionResult> Received()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var inquiries = await _inquiryService.GetInquiriesByReceiverAsync(user.Id);
        ViewBag.UnreadCount = await _inquiryService.GetUnreadCountByReceiverAsync(user.Id);
        return View(inquiries);
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var inquiry = await _inquiryService.GetInquiryByIdAsync(id);
        if (inquiry == null)
        {
            return NotFound();
        }

        // Check if user has access to this inquiry
        if (inquiry.SenderId != user.Id && inquiry.ReceiverId != user.Id)
        {
            return Forbid();
        }

        // Mark as read if user is the receiver
        if (inquiry.ReceiverId == user.Id && !inquiry.IsRead)
        {
            await _inquiryService.MarkAsReadAsync(id);
            inquiry.IsRead = true;
        }

        return View(inquiry);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var inquiry = await _inquiryService.GetInquiryByIdAsync(id);
        if (inquiry == null)
        {
            return NotFound();
        }

        // Only sender can delete their inquiry
        if (inquiry.SenderId != user.Id)
        {
            return Forbid();
        }

        await _inquiryService.DeleteInquiryAsync(id);
        TempData["Success"] = "Inquiry deleted successfully.";
        return RedirectToAction(nameof(Sent));
    }
}

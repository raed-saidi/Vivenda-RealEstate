using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.BLL.Services;
using RealEstateMarketplace.DAL.Entities;

namespace RealEstateMarketplace.Web.Controllers;

public class PropertiesController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly ICategoryService _categoryService;
    private readonly IAmenityService _amenityService;
    private readonly IInquiryService _inquiryService;
    private readonly UserManager<User> _userManager;

    public PropertiesController(
        IPropertyService propertyService,
        ICategoryService categoryService,
        IAmenityService amenityService,
        IInquiryService inquiryService,
        UserManager<User> userManager)
    {
        _propertyService = propertyService;
        _categoryService = categoryService;
        _amenityService = amenityService;
        _inquiryService = inquiryService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(PropertySearchDto? search)
    {
        IEnumerable<PropertyDto> properties;
        
        if (search != null && HasSearchCriteria(search))
        {
            properties = await _propertyService.SearchPropertiesAsync(search);
        }
        else
        {
            properties = await _propertyService.GetAllPropertiesAsync();
            properties = properties.Where(p => p.Status == "Active");
        }
        
        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Search = search ?? new PropertySearchDto();
        
        return View(properties);
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await _propertyService.GetPropertyByIdAsync(id);
        if (property == null)
        {
            return NotFound();
        }
        
        // Get related properties (same city or type)
        var search = new PropertySearchDto { City = property.City };
        var relatedProperties = (await _propertyService.SearchPropertiesAsync(search))
            .Where(p => p.Id != id)
            .Take(3);
        
        ViewBag.RelatedProperties = relatedProperties;
        
        return View(property);
    }
    
    private bool HasSearchCriteria(PropertySearchDto search)
    {
        return !string.IsNullOrEmpty(search.Keyword) ||
               !string.IsNullOrEmpty(search.PropertyType) ||
               !string.IsNullOrEmpty(search.ListingType) ||
               !string.IsNullOrEmpty(search.City) ||
               search.MinPrice.HasValue ||
               search.MaxPrice.HasValue ||
               search.MinBedrooms.HasValue ||
               search.MaxBedrooms.HasValue;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendInquiry(CreateInquiryDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction(nameof(Details), new { id = dto.PropertyId });
        }

        try
        {
            // Get the property to find the receiver (property owner)
            var property = await _propertyService.GetPropertyByIdAsync(dto.PropertyId);
            if (property == null)
            {
                TempData["Error"] = "Property not found.";
                return RedirectToAction(nameof(Index));
            }

            // Set receiver as property owner
            dto.ReceiverId = property.UserId;

            // If user is logged in, set sender ID
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                dto.SenderId = user?.Id;
            }

            await _inquiryService.CreateInquiryAsync(dto);
            TempData["Success"] = "Your inquiry has been sent successfully! The agent will contact you soon.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to send inquiry: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = dto.PropertyId });
    }
}

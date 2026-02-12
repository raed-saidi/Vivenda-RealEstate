using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.BLL.Services;
using RealEstateMarketplace.DAL.Entities;
using RealEstateMarketplace.Web.Areas.Admin.Models;

namespace RealEstateMarketplace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PropertiesController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly ICategoryService _categoryService;
    private readonly IAmenityService _amenityService;
    private readonly UserManager<User> _userManager;

    public PropertiesController(
        IPropertyService propertyService,
        ICategoryService categoryService,
        IAmenityService amenityService,
        UserManager<User> userManager)
    {
        _propertyService = propertyService;
        _categoryService = categoryService;
        _amenityService = amenityService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var properties = await _propertyService.GetAllPropertiesAsync();
        return View(properties);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateViewBags();
        return View(new PropertyFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User);
            var dto = new CreatePropertyDto
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Address = model.Address,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                Country = model.Country,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                SquareFeet = model.SquareFeet,
                YearBuilt = model.YearBuilt,
                PropertyType = model.PropertyType,
                ListingType = model.ListingType,
                MainImageUrl = model.MainImageUrl,
                CategoryId = model.CategoryId,
                AmenityIds = model.SelectedAmenityIds ?? new List<int>()
            };
            
            await _propertyService.CreatePropertyAsync(dto, userId!);
            TempData["Success"] = "Property created successfully!";
            return RedirectToAction(nameof(Index));
        }
        
        await PopulateViewBags();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var property = await _propertyService.GetPropertyByIdAsync(id);
        if (property == null)
        {
            return NotFound();
        }
        
        var model = new PropertyFormViewModel
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price,
            Address = property.Address,
            City = property.City,
            State = property.State,
            ZipCode = property.ZipCode,
            Country = property.Country,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            SquareFeet = property.SquareFeet,
            YearBuilt = property.YearBuilt,
            PropertyType = property.PropertyType,
            ListingType = property.ListingType,
            Status = property.Status,
            MainImageUrl = property.MainImageUrl,
            IsFeatured = property.IsFeatured,
            CategoryId = property.CategoryId,
            SelectedAmenityIds = property.AmenityIds
        };
        
        await PopulateViewBags();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PropertyFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }
        
        if (ModelState.IsValid)
        {
            var dto = new UpdatePropertyDto
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Address = model.Address,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                Country = model.Country,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                SquareFeet = model.SquareFeet,
                YearBuilt = model.YearBuilt,
                PropertyType = model.PropertyType,
                ListingType = model.ListingType,
                Status = model.Status,
                MainImageUrl = model.MainImageUrl,
                IsFeatured = model.IsFeatured,
                CategoryId = model.CategoryId,
                AmenityIds = model.SelectedAmenityIds ?? new List<int>()
            };
            
            await _propertyService.UpdatePropertyAsync(dto);
            TempData["Success"] = "Property updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        
        await PopulateViewBags();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _propertyService.DeletePropertyAsync(id);
        TempData["Success"] = "Property deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFeatured(int id)
    {
        var isFeatured = await _propertyService.ToggleFeaturedAsync(id);
        return Json(new { success = true, isFeatured });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var success = await _propertyService.UpdateStatusAsync(id, status);
        return Json(new { success });
    }

    private async Task PopulateViewBags()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        var amenities = await _amenityService.GetAllAmenitiesAsync();
        
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        ViewBag.Amenities = amenities;
        ViewBag.PropertyTypes = new SelectList(Enum.GetNames(typeof(PropertyType)));
        ViewBag.ListingTypes = new SelectList(Enum.GetNames(typeof(ListingType)));
        ViewBag.Statuses = new SelectList(Enum.GetNames(typeof(PropertyStatus)));
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.BLL.Services;
using RealEstateMarketplace.Web.Models;

namespace RealEstateMarketplace.Web.Controllers;

public class HomeController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly ICategoryService _categoryService;

    public HomeController(IPropertyService propertyService, ICategoryService categoryService)
    {
        _propertyService = propertyService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var featuredProperties = await _propertyService.GetFeaturedPropertiesAsync(6);
        var latestProperties = await _propertyService.GetLatestPropertiesAsync(8);
        var categories = await _categoryService.GetAllCategoriesAsync();
        
        var viewModel = new HomeViewModel
        {
            FeaturedProperties = featuredProperties.ToList(),
            LatestProperties = latestProperties.ToList(),
            Categories = categories.ToList()
        };
        
        return View(viewModel);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

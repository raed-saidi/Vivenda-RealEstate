using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.BLL.Services;

namespace RealEstateMarketplace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View(new CategoryDto { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryDto model)
    {
        if (ModelState.IsValid)
        {
            await _categoryService.CreateCategoryAsync(model);
            TempData["Success"] = "Category created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryDto model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }
        
        if (ModelState.IsValid)
        {
            await _categoryService.UpdateCategoryAsync(model);
            TempData["Success"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        TempData["Success"] = "Category deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}

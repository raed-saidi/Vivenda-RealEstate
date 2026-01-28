using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateMarketplace.DAL.Entities;
using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<User> _userManager;

    public UsersController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            });
        }

        return View(userDtos);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Json(new { success = false });
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        return Json(new { success = true, isActive = user.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == id)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
            TempData["SuccessMessage"] = "User deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}

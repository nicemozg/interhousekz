using English_games.Context;
using English_games.Models;
using English_games.Services;
using English_games.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace English_games.Controllers;

public class UsersController : Controller
{
    private English_gamesContext _db;
    private readonly UserManager<User> _userManager;
    private readonly MobizonApiService _mobizonApiService;

    public UsersController(English_gamesContext db, UserManager<User> userManager, MobizonApiService mobizonApiService)
    {
        _db = db;
        _userManager = userManager;
        _mobizonApiService = mobizonApiService;
    }

    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) throw new ArgumentNullException(nameof(currentUser));

        var users = _db.Users
            .Where(user => user.UserName != "7064441111" && user.Id != currentUser.Id)
            .OrderBy(user => user.IntUserId)
            .Select(user => new UserViewModel
            {
                Id = user.Id,
                IntUserId = user.IntUserId,
                UserName = user.UserName,
                Creation = user.Creation,
                Balance = user.Balance,
                Block = user.Block,
                Admin = user.Admin,
                PurchaseThemes = user.Purchases.Count() // Подсчет количества покупок пользователя
            })
            .ToList<UserViewModel>(); // Явное указание типа UserViewModel

        return View(users);
    }



    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> ToggleUserRole(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"User with UserName {user.UserName} not found.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        IdentityResult resultAdd;

        if (currentRoles.Contains("admin"))
        {
            // Пользователь имеет роль "admin", удаляем её
            resultAdd = await _userManager.RemoveFromRoleAsync(user, "admin");
            if (resultAdd.Succeeded)
            {
                // Устанавливаем роль "user"
                resultAdd = await _userManager.AddToRoleAsync(user, "user");
            }
        }
        else
        {
            // Пользователь имеет роль "user", удаляем её
            resultAdd = await _userManager.RemoveFromRoleAsync(user, "user");
            if (resultAdd.Succeeded)
            {
                // Устанавливаем роль "admin"
                resultAdd = await _userManager.AddToRoleAsync(user, "admin");
            }
        }

        if (resultAdd.Succeeded)
        {
            user.Admin = !currentRoles.Contains("admin");
            _db.Update(user);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        return BadRequest($"Failed to toggle user role. Errors: {string.Join(", ", resultAdd.Errors)}");
    }
    
    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> ToggleBlockUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"User with UserName {user.UserName} not found.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        IdentityResult resultAdd;

        if (currentRoles.Contains("block"))
        {
            // Пользователь имеет роль "block", удаляем её
            resultAdd = await _userManager.RemoveFromRoleAsync(user, "block");
            if (resultAdd.Succeeded)
            {
                // Устанавливаем роль "user"
                resultAdd = await _userManager.AddToRoleAsync(user, "user");
            }
        }
        else
        {
            // Пользователь имеет роль "user", удаляем её
            resultAdd = await _userManager.RemoveFromRoleAsync(user, "user");
            if (resultAdd.Succeeded)
            {
                // Устанавливаем роль "block"
                resultAdd = await _userManager.AddToRoleAsync(user, "block");
            }
        }

        if (resultAdd.Succeeded)
        {
            user.Block = !currentRoles.Contains("block");
            _db.Update(user);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        return BadRequest($"Failed to toggle user role. Errors: {string.Join(", ", resultAdd.Errors)}");
    }
    
    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> Pay(string userId)
    {
        UserViewModel userViewModel = new UserViewModel
        {
            Id = userId
        };
        return View(userViewModel);
    }

    [HttpPost]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> Pay(UserViewModel model)
    {
        if (string.IsNullOrEmpty(model.Id) || model.ReplenishmentAmount <= 0)
        {
            ViewBag.Error = "Сумма поплнения должна быть больше 0";
            return RedirectToAction("Pay");
        }

        if (model.ReplenishmentAmount > 0)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            user.Balance = user.Balance + model.ReplenishmentAmount;
            _db.Users.Update(user);
            _db.SaveChanges();

        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> ReturnPay(string userId)
    {
        UserViewModel userViewModel = new UserViewModel
        {
            Id = userId
        };
        return View(userViewModel);
    }

    [HttpPost]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> ReturnPay(UserViewModel model)
    {
        
        if (string.IsNullOrEmpty(model.Id) || model.ReplenishmentAmount <= 0)
        {
            ViewBag.Error = "Сумма списания должна быть больше 0";
            return RedirectToAction("Pay");
        }
        var user = await _userManager.FindByIdAsync(model.Id);
        if (model.ReplenishmentAmount > 0 && user.Balance >= model.ReplenishmentAmount)
        {
            user.Balance = user.Balance - model.ReplenishmentAmount;
            _db.Users.Update(user);
            _db.SaveChanges();
        }

        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> BlockPage()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult>  Wallet()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(currentUser, "block"))
        {
            return RedirectToAction("BlockPage", "Users");
        }
        if (!currentUser.PhoneNumberConfirmed)
        {
            return RedirectToAction("ConfirmedPhoneNumber", "Account");
        }
        UserViewModel userViewModel = new UserViewModel();
        userViewModel.Balance = currentUser.Balance;
        return View(userViewModel);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> PayFromMalpay(int sum)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        currentUser.Balance = currentUser.Balance + sum;
            _db.Users.Update(currentUser);
            _db.SaveChanges();
        return RedirectToAction("Index", "MyTheme");
    }
    
    [Authorize]
    [HttpGet]
    public async Task SendSmsForPay()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var phoneNumber = "77089238200";
        var text = $"Абонент с Id {currentUser.IntUserId} номером {currentUser.PhoneNumber} хочет пополнить баланс";
        
        await _mobizonApiService.SendMessageAsync(
            phoneNumber, text);
        
        await _mobizonApiService.BalanceInMobizon();
    }
}
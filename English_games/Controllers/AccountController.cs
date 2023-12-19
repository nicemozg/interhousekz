using System.Text;
using System.Text.Json;
using English_games.Context;
using English_games.Models;
using English_games.Services;
using English_games.ViewModels.Account;
using English_games.ViewModels.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace English_games.Controllers;

public class AccountController : Controller
{
    private readonly English_gamesContext _db;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly MobizonApiService _mobizonApiService;

    public AccountController(English_gamesContext db,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IWebHostEnvironment hostingEnvironment,
        MobizonApiService mobizonApiService)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _hostingEnvironment = hostingEnvironment;
        _mobizonApiService = mobizonApiService;
    }


    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        string processedPhoneNumber = new string(model.PhoneNumber.Where(char.IsDigit).ToArray());
        var user = await _userManager.FindByNameAsync(processedPhoneNumber);
        if (user is not null)
        {
            ViewBag.Error = $"Номер {user.PhoneNumber} уже зарегистрирован";
            return View(model);
        }
        // var userEmail = await _userManager.FindByEmailAsync(model.Email);
        // if (userEmail is not null)
        // {
        //     ViewBag.Error = "Такая почта занята";
        //     return View(model);
        // }

        // if (ModelState.IsValid)
        // {
        // if (model.AvatarFile != null && model.AvatarFile.Length > 0)
        // {
        //     string extension = Path.GetExtension(model.AvatarFile.FileName);
        //     if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        //     {
        //         ViewBag.Error = "Расширение файла должно быть .jpg, .jpeg или .png";
        //         return View();
        //     }
        //     
        //     string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "avatars");
        //     if (!Directory.Exists(uploadsFolder))
        //     {
        //         Directory.CreateDirectory(uploadsFolder);
        //     }
        //
        //     string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.AvatarFile.FileName;
        //     string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        //     using (var stream = new FileStream(filePath, FileMode.Create))
        //     {
        //         model.AvatarFile.CopyTo(stream);
        //     }
        //
        //     model.AvatarFileName = uniqueFileName;
        // }
        // else
        // {
        //     model.AvatarFileName =
        //         "92997796-c484-4287-be0b-5d7ab7432a1f_No Image.jpg";
        // }

        var maxIntUserId = _db.Users.Max(u => u.IntUserId);
        User newUser = new User
        {
            UserName = processedPhoneNumber,
            // Email = model.Email,
            // AvatarFileName = model.AvatarFileName,
            Name = model.Name,
            PhoneNumber = $"7{processedPhoneNumber}",
            Creation = DateTime.Now,
            Balance = 0,
            IntUserId = maxIntUserId + 1,
            Adult = model.Adult
        };
        var result = await _userManager.CreateAsync(newUser, model.Password);
        if (result.Succeeded)
        {
            var codeNumber = new Random().Next(1000, 9999).ToString();
            var passwordResetViewModel = new PasswordResetViewModel
            {
                recipient = newUser.PhoneNumber,
                text = $"Код подтверждения номера для InterHouse {codeNumber}",
            };
            newUser.CodeNumber = codeNumber;
            _db.Users.Update(newUser);
            _db.SaveChangesAsync();


            await _mobizonApiService.SendMessageAsync(passwordResetViewModel.recipient,
                passwordResetViewModel.text);


            await _mobizonApiService.BalanceInMobizon();
            await _userManager.AddToRoleAsync(newUser, "user");
            await _signInManager.SignInAsync(newUser, false);
            return RedirectToAction("Index", "MyTheme");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);
        // }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.FindByNameAsync(model.EmailOrUserName);
            if (user is null)
            {
                ViewBag.Error = "Не верный логин или пароль";
                return View(model);
            }

            if (user is not null)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return RedirectToAction(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "MyTheme");
                }
            }
        }

        ViewBag.Error = "Не верный логин или пароль";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
        {
            ViewBag.NotFoundUser = "Данный номер не зарегистрирован";
            return View("ForgotPasswordConfirmation");
        }

        if (!(await _userManager.IsPhoneNumberConfirmedAsync(user)))
        {
            return RedirectToAction("ConfirmedPhoneNumber");
        }

        var codeNumber = new Random().Next(1000, 9999).ToString();
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code },
            protocol: HttpContext.Request.Scheme);
        ViewBag.Token = callbackUrl;
        ViewBag.Code = codeNumber;
        TempData["code"] = codeNumber;
        TempData["userName"] = user.UserName;
        user.CodeNumber = codeNumber;
        _db.Users.Update(user);
        _db.SaveChanges();

        var passwordResetViewModel = new PasswordResetViewModel
        {
            recipient = user.PhoneNumber,
            text = $"Код сброса пароля для InterHouse {codeNumber}", // Ваш текст сообщения
        };

        await _mobizonApiService.SendMessageAsync(
            passwordResetViewModel.recipient,
            passwordResetViewModel.text);

        await _mobizonApiService.BalanceInMobizon();


        // EmailService emailService = new EmailService();
        // await emailService.SendEmailAsync(model.Email, "Reset Password",
        // $"Для сброса пароля пройдите по ссылке: <a href='{callbackUrl}'>link</a>");
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string code = null)
    {
        return code == null ? View("Error") : View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
        {
            TempData["userName"] = model.UserName;
            ViewBag.Error = "Не верный код";
            return View();
        }

        if (user.CodeNumber == model.CodeNumberForResetPassword)
        {
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        else
        {
            TempData["userName"] = model.UserName;
            ViewBag.Error = "Не верный код";
            return View();
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmedPhoneNumber()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmedPhoneNumber(ConfirmedPhoneNumberViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser.CodeNumber == model.CodeForConfirmNumber)
        {
            currentUser.PhoneNumberConfirmed = true;
            _db.Users.Update(currentUser);
            _db.SaveChanges();
            return RedirectToAction("Index", "MyTheme");
        }
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendSms()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var codeNumber = new Random().Next(1000, 9999).ToString();
        currentUser.CodeNumber = codeNumber;
        _db.Users.Update(currentUser);
        _db.SaveChanges();
        var passwordResetViewModel = new PasswordResetViewModel
        {
            recipient = currentUser.PhoneNumber,
            text = $"Код подтверждения номера для InterHouse {codeNumber}", // Ваш текст сообщения
        };


        await _mobizonApiService.SendMessageAsync(
            passwordResetViewModel.recipient,
            passwordResetViewModel.text);
        await _mobizonApiService.BalanceInMobizon();

        return RedirectToAction("ConfirmedPhoneNumber");
    }

    [HttpGet]
    public IActionResult PersonalData()
    {
        return View();
    }
}
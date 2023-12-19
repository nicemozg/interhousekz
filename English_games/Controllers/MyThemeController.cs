using System.Text.Json;
using System.Text.RegularExpressions;
using English_games.Context;
using English_games.Enum;
using English_games.Models;
using English_games.ViewModels;
using English_games.ViewModels.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace English_games.Controllers;

public class MyThemeController : Controller
{
    private English_gamesContext _db;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public MyThemeController(English_gamesContext db, UserManager<User> userManager,
        IWebHostEnvironment hostingEnvironment)
    {
        _db = db;
        _userManager = userManager;
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> AnonimusIndex(int page = 1, int pageSize = 6)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null)
        {
            return RedirectToAction("Index");
        }
    
        var themes = _db.MyThemes.OrderByDescending(t => t.ClickCount).ToList();
        var pagedThemes = themes.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    
        // Преобразование MyTheme в MyThemViewModel
        var myThemViewModels = pagedThemes.Select(myTheme => new MyThemViewModel(myTheme)).ToList();

    
        // Создание списка ViewModel для отображения
        var paginationViewModel = new PaginationThemeViewModel
        {
            Themes = myThemViewModels,
            TotalCount = themes.Count,
            CurrentPage = page,
            PageSize = pageSize
        };

        return View(paginationViewModel);
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 6)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        // Проверка на блокировку пользователя
        if (await _userManager.IsInRoleAsync(currentUser, "block"))
        {
            return RedirectToAction("BlockPage", "Users");
        }

        // Проверка подтверждения номера телефона
        if (!currentUser.PhoneNumberConfirmed)
        {
            return RedirectToAction("ConfirmedPhoneNumber", "Account");
        }

        // Проверка наличия пользователя и перенаправление на страницу регистрации, если необходимо
        if (string.IsNullOrEmpty(currentUser?.Id))
        {
            return RedirectToAction("Register", "Account");
        }

        // Загрузка всех тем с покупками
        var themesQuery = _db.MyThemes
            .Include(theme => theme.Purchases)
            .OrderBy(t => t.ThemeNumber);

        var totalCount = await themesQuery.CountAsync();
        var themes = await themesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Получение максимального номера темы и списка покупок пользователя
        var maxThemeNumber = _db.MyThemes
            .Select(theme => theme.ThemeNumber)
            .DefaultIfEmpty() // Вернет 0, если коллекция пуста
            .Max();

        var purchaseList = await _db.Purchases
            .Where(purchase => purchase.UserId == currentUser.Id)
            .ToListAsync();

        if (purchaseList.Count != 0)
        {
            // Вычисление пройденных и оставшихся тем
            var maxPurchaseThemeNumber = await _db.Purchases.Include(p => p.MyTheme)
                .Where(purchase => purchase.UserId == currentUser.Id)
                .MaxAsync(purchase => purchase.MyTheme.ThemeNumber);
            var minPurchaseThemeNumber = await _db.Purchases.Include(p => p.MyTheme)
                .Where(purchase => purchase.UserId == currentUser.Id)
                .MinAsync(purchase => purchase.MyTheme.ThemeNumber);
            var passedTheme = purchaseList.Count - 1;
            var reminderTheme = maxThemeNumber - maxPurchaseThemeNumber;

            ViewBag.PassedTheme = passedTheme;
            ViewBag.ReminderTheme = reminderTheme.ToString();
        }

        // Создание списка ViewModel для отображения
        var paginationViewModel = new PaginationThemeViewModel
        {
            Themes = themes
                .Select(theme => new MyThemViewModel(theme)
                {
                    AccessTo = purchaseList
                        .Where(purchase => purchase.MyThemeId == theme.Id)
                        .OrderByDescending(purchase => purchase.AccessTo)
                        .FirstOrDefault()?.AccessTo,
                    Adult = currentUser.Adult,
                })
                .ToList(),
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };

        // Установка цитаты и баланса в ViewBag
        var quote = _db.Quotes.FirstOrDefault(q => q.Id == 1);
        ViewBag.Quote = quote?.Text;
        ViewBag.Balance = currentUser.Balance.ToString();

        // Установка баланса Mobizon в ViewBag
        var mobizoneBalance = _db.Mobizons.FirstOrDefault(m => m.Id == 1);
        ViewBag.MobizoneBalance = mobizoneBalance?.Balance;
        return View(paginationViewModel);
    }


    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult AddTheme()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult AddTheme(MyThemViewModel model)
    {
        string youtubeUrl_16_Plus = model.LinkForVideo_16_Plus;
        youtubeUrl_16_Plus = youtubeUrl_16_Plus.Replace("https://youtu.be/", "");
        model.LinkForVideo_16_Plus = youtubeUrl_16_Plus;

        string youtubeUrl_16_Minus = model.LinkForVideo_16_Minus;
        youtubeUrl_16_Minus = youtubeUrl_16_Minus.Replace("https://youtu.be/", "");
        model.LinkForVideo_16_Minus = youtubeUrl_16_Minus;

        if (model.PreviewFile != null && model.PreviewFile.Length > 0)
        {
            string extension = Path.GetExtension(model.PreviewFile.FileName);
            if (extension != ".jpg"
                && extension != ".JPG"
                && extension != ".Jpg"
                && extension != ".jpeg"
                && extension != ".Jpeg"
                && extension != ".JPEG"
                && extension != ".png"
                && extension != ".Png"
                && extension != ".PNG")
            {
                ViewBag.Error = "Расширение файла должно быть .jpg, JPG, .jpeg, JPEG .png, PNG";
                return View();
            }

            string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images/previews");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.PreviewFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                model.PreviewFile.CopyTo(stream);
            }

            model.LinkForPreview = uniqueFileName;
        }
        else
        {
            model.LinkForPreview =
                "No Photo.png";
        }

        MyTheme myTheme = new MyTheme
        {
            Name = model.Name,
            ThemeNumber = model.ThemeNumber,
            LinkForVideo_16_Plus = model.LinkForVideo_16_Plus,
            LinkForVideo_16_Minus = model.LinkForVideo_16_Minus,
            CreatedAt = DateTime.Now,
            Price_3_Months = model.Price_3_Months,
            Price_6_Months = model.Price_6_Months,
            Price_Infinity = model.Price_Infinity,
            LinkForPreview = model.LinkForPreview,
            ClickCount = 0,
            FakePurchaseCount = 0
        };
        if (model.PurchaseCount > 0)
        {
            myTheme.FakePurchaseCount = model.PurchaseCount;
        }

        _db.MyThemes.Add(myTheme);
        _db.SaveChanges();
        return RedirectToAction("Index");


        // ViewBag.Error = "Не все поля заполнены";
        // return RedirectToAction("AddTheme");
    }

    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult EditTheme(int themId)
    {
        var myTheme = _db.MyThemes.FirstOrDefault(t => t.Id == themId);
        MyThemViewModel myThemViewModel = new MyThemViewModel
        {
            Id = myTheme.Id,
            ThemeNumber = myTheme.ThemeNumber,
            Name = myTheme.Name,
            LinkForVideo_16_Plus = myTheme.LinkForVideo_16_Plus,
            LinkForVideo_16_Minus = myTheme.LinkForVideo_16_Minus,
            Price_3_Months = myTheme.Price_3_Months,
            Price_6_Months = myTheme.Price_6_Months,
            Price_Infinity = myTheme.Price_Infinity,
            PurchaseCount = myTheme.FakePurchaseCount
        };
        return View(myThemViewModel);
    }

    [HttpPost]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult EditTheme(MyThemViewModel model)
    {
        var myTheme = _db.MyThemes.FirstOrDefault(t => t.Id == model.Id);
        string youtubeUrl = model.LinkForVideo_16_Plus;
        youtubeUrl = youtubeUrl.Replace("https://youtu.be/", "");
        model.LinkForVideo_16_Plus = youtubeUrl;

        if (model.PreviewFile != null && model.PreviewFile.Length > 0)
        {
            string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images/previews", myTheme.LinkForPreview);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            string extension = Path.GetExtension(model.PreviewFile.FileName);
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                ViewBag.Error = "Расширение файла должно быть .jpg, .jpeg или .png";
                return View();
            }

            string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images/previews");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.PreviewFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                model.PreviewFile.CopyTo(stream);
            }

            model.LinkForPreview = uniqueFileName;
        }
        else
        {
            model.LinkForPreview = myTheme.LinkForPreview;
        }

        myTheme.Name = model.Name;
        myTheme.ThemeNumber = model.ThemeNumber;
        myTheme.LinkForVideo_16_Plus = model.LinkForVideo_16_Plus;
        myTheme.LinkForVideo_16_Minus = model.LinkForVideo_16_Minus;
        myTheme.LinkForPreview = model.LinkForPreview;
        myTheme.Price_3_Months = model.Price_3_Months;
        myTheme.Price_6_Months = model.Price_6_Months;
        myTheme.Price_Infinity = model.Price_Infinity;
        if (model.PurchaseCount >= 0)
        {
            myTheme.FakePurchaseCount = model.PurchaseCount;
        }

        _db.MyThemes.Update(myTheme);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> DeleteTheme(int themeId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (string.IsNullOrEmpty(currentUser?.Id))
        {
            return RedirectToAction("Register", "Account");
        }

        var purchaseTheme = _db.Purchases.Include(t => t.MyTheme).FirstOrDefault(p => p.MyThemeId == themeId);
        if (purchaseTheme != null)
        {
            TempData["PurchaseTheme"] =
                $"Данная тема {purchaseTheme.MyTheme.Name} не может быть удалена так как она куплена";
            return RedirectToAction("Index");
        }

        var theme = _db.MyThemes.Include(t => t.LinkForGames)
            .Include(t => t.LinkForBooks)
            .FirstOrDefault(t => t.Id == themeId);

        if (theme == null)
        {
            return RedirectToAction("Index");
        }

        string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images/previews", theme.LinkForPreview);
        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }

        _db.LinkForGames.RemoveRange(theme.LinkForGames);
        _db.LinkForBooks.RemoveRange(theme.LinkForBooks);
        var purchasesToRemove = _db.Purchases.Where(p => p.MyThemeId == themeId);


        _db.Purchases.RemoveRange(purchasesToRemove);
        _db.MyThemes.Remove(theme);

        _db.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> AnonimusDetailTheme(int themeId)
    {
        var theme = _db.MyThemes
            .Include(t => t.LinkForGames.OrderBy(t => t.Game))
            .Include(t => t.LinkForBooks)
            .FirstOrDefault(t => t.Id == themeId);

        theme.ClickCount = theme.ClickCount + 1;
        _db.MyThemes.Update(theme);
        _db.SaveChanges();


        var themeViewModel = new MyThemViewModel()
        {
            Id = theme.Id,
            Name = theme.Name,
            CreatedAtMyTheme = theme.CreatedAt,
            LinkForVideo_16_Plus = theme.LinkForVideo_16_Plus,
            LinkForGames = theme.LinkForGames,
            LinkForBooks = theme.LinkForBooks,
            LinkForPreview = theme.LinkForPreview,
            Purchase = false,
        };

        return View(themeViewModel);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DetailTheme(int themeId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(currentUser, "block"))
        {
            return RedirectToAction("BlockPage", "Users");
        }

        if (string.IsNullOrEmpty(currentUser?.Id))
        {
            return RedirectToAction("Register", "Account");
        }

        var theme = _db.MyThemes
            .Include(t => t.LinkForGames.OrderBy(g => g.Game))
            .Include(t => t.LinkForBooks)
            .FirstOrDefault(t => t.Id == themeId);


        theme.ClickCount = theme.ClickCount + 1;
        _db.MyThemes.Update(theme);
        _db.SaveChanges();
        var purchaseTheme = _db.Purchases.FirstOrDefault(p
            => p.MyThemeId == themeId && p.UserId == currentUser.Id);

        var themeViewModel = new MyThemViewModel()
        {
            Id = theme.Id,
            Name = theme.Name,
            CreatedAtMyTheme = theme.CreatedAt,
            LinkForVideo_16_Plus = theme.LinkForVideo_16_Plus,
            LinkForVideo_16_Minus = theme.LinkForVideo_16_Minus,
            LinkForGames = theme.LinkForGames,
            LinkForBooks = theme.LinkForBooks,
            LinkForPreview = theme.LinkForPreview,
            Adult = currentUser.Adult
        };
        if (purchaseTheme is not null)
        {
            themeViewModel.AccessTo = purchaseTheme.AccessTo;
        }

        ViewBag.Balance = currentUser.Balance;
        return View(themeViewModel);
    }

    [Authorize(Roles = "admin,superAdmin")]
    [HttpPost]
    public IActionResult AddGame(AddGameViewModel model)
    {
        if (ModelState.IsValid)
        {
            LinkForGame linkForGame = new LinkForGame
            {
                Name = model.Name,
                LinkPathForGame = model.LinkPathForGame,
                CreatedAt = DateTime.Now,
                MyThemeId = model.ThemId,
                DescriptionForGame = model.DescriptionForGame
            };
            if (model.Game == Game.Web) ;
            {
                linkForGame.Game = Game.Web;
            }
            if (model.Game == Game.Ios)
            {
                linkForGame.Game = Game.Ios;
            }

            if (model.Game == Game.Android)
            {
                linkForGame.Game = Game.Android;
            }

            _db.LinkForGames.Add(linkForGame);
            _db.SaveChanges();
            return RedirectToAction("DetailTheme", "MyTheme", new { themeId = model.ThemId });
        }
        TempData["Error"] = "Все поля должны быть заполнены";
        return RedirectToAction("DetailTheme", "MyTheme", new { themeId = model.ThemId });
    }

    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult EditGame(int gameId)
    {
        if (gameId > 0)
        {
            var game = _db.LinkForGames.FirstOrDefault(g => g.Id == gameId);
            AddGameViewModel addGameViewModel = new AddGameViewModel(game);
            return View(addGameViewModel);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult EditGame(AddGameViewModel model)
    {
        if (ModelState.IsValid)
        {
            var game = _db.LinkForGames.FirstOrDefault(g => g.Id == model.Id);
            game.Name = model.Name;
            game.LinkPathForGame = model.LinkPathForGame;
            game.Game = model.Game;
            game.DescriptionForGame = model.DescriptionForGame;
            _db.LinkForGames.Update(game);
            _db.SaveChanges();
            return RedirectToAction("DetailTheme", "MyTheme", new { themeId = game.MyThemeId });
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult DeleteGame(int gameId)
    {
        var game = _db.LinkForGames.FirstOrDefault(t => t.Id == gameId);
        _db.LinkForGames.Remove(game);
        _db.SaveChanges();
        return RedirectToAction("DetailTheme", "MyTheme", new { themeId = game.MyThemeId });
    }

    [HttpPost]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult AddBook(AddBookViewModel model)
    {
        if (ModelState.IsValid)
        {
            LinkForBook linkForBook = new LinkForBook
            {
                Name = model.Name,
                LinkPathForBook = model.LinkPathForBook,
                CreatedAt = DateTime.Now,
                MyThemeId = model.ThemeId
            };
            _db.LinkForBooks.Add(linkForBook);
            _db.SaveChanges();
            return RedirectToAction("DetailTheme", "MyTheme", new { themeId = model.ThemeId });
        }

        TempData["Error"] = "Все поля должны быть заполнены";
        return RedirectToAction("DetailTheme", "MyTheme", new { themeId = model.ThemeId });
    }

    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult EditBook(int bookId)
    {
        if (bookId > 0)
        {
            var book = _db.LinkForBooks.FirstOrDefault(g => g.Id == bookId);
            AddBookViewModel addBookViewModel = new AddBookViewModel(book);
            return View(addBookViewModel);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult EditBook(AddBookViewModel model)
    {
        if (ModelState.IsValid)
        {
            var book = _db.LinkForBooks.FirstOrDefault(b => b.Id == model.Id);
            book.Name = model.Name;
            book.LinkPathForBook = model.LinkPathForBook;
            _db.LinkForBooks.Update(book);
            _db.SaveChanges();
            return RedirectToAction("DetailTheme", "MyTheme", new { themeId = book.MyThemeId });
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public IActionResult DeleteBook(int bookId)
    {
        var book = _db.LinkForBooks.FirstOrDefault(t => t.Id == bookId);
        _db.LinkForBooks.Remove(book);
        _db.SaveChanges();
        return RedirectToAction("DetailTheme", "MyTheme", new { themeId = book.MyThemeId });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PurchaseTheme(PurchaseThemeViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(currentUser, "block"))
        {
            return RedirectToAction("BlockPage", "Users");
        }

        if (string.IsNullOrEmpty(currentUser?.Id))
        {
            return RedirectToAction("Register", "Account");
        }

        var theme = _db.MyThemes.FirstOrDefault(t => t.Id == model.ThemId);

        if (currentUser.Balance < theme.Price_3_Months && model.AccessTo == "3_months")
        {
            TempData["Error"] = "Недостаточно средств";
            return RedirectToAction("Index");
        }

        if (currentUser.Balance < theme.Price_6_Months && model.AccessTo == "6_months")
        {
            TempData["Error"] = "Недостаточно средств";
            return RedirectToAction("Index");
        }

        if (currentUser.Balance < theme.Price_Infinity && model.AccessTo == "Infinity")
        {
            TempData["Error"] = "Недостаточно средств";
            return RedirectToAction("Index");
        }

        Purchase purchase = new Purchase();
        if (model.AccessTo == "3_months")
        {
            currentUser.Balance = currentUser.Balance - theme.Price_3_Months;
            purchase.AccessTo = DateTime.Today.AddDays(93);
        }
        else if (model.AccessTo == "6_months")
        {
            currentUser.Balance = currentUser.Balance - theme.Price_6_Months;
            purchase.AccessTo = DateTime.Today.AddDays(186);
        }
        else
        {
            currentUser.Balance = currentUser.Balance - theme.Price_Infinity;
            purchase.AccessTo = DateTime.Today.AddDays(360000);
        }


        theme.FakePurchaseCount += 1;
        theme.RealPurchaseCount += 1;
        purchase.UserId = currentUser.Id;
        purchase.MyThemeId = theme.Id;
        var purchaseRemove = _db.Purchases.FirstOrDefault
            (p => p.UserId == currentUser.Id && p.MyThemeId == model.ThemId);
        if (purchaseRemove != null)
        {
            _db.Purchases.Remove(purchaseRemove);
        }

        _db.Purchases.Add(purchase);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyPurchaseThems()
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

        if (string.IsNullOrEmpty(currentUser?.Id))
        {
            return RedirectToAction("Register", "Account");
        }

        var purchasedThemes = await _db.Purchases
            .Where(purchase => purchase.UserId == currentUser.Id)
            .Select(purchase => purchase.MyTheme)
            .ToListAsync();

        return View(purchasedThemes);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> UserPurchaseThems(string UserId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(currentUser, "block"))
        {
            return RedirectToAction("BlockPage", "Users");
        }

        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToAction("Register", "Account");
        }

        var user = _db.Users.FirstOrDefault(u => u.Id == UserId);
        var purchasedThemes = await _db.Purchases.Include(p => p.MyTheme)
            .Where(purchase => purchase.UserId == UserId)
            .ToListAsync();
        if (purchasedThemes.Count == 0)
        {
            TempData["NullThemes"] = $"У пользователя {user.PhoneNumber} нет купленных тем";
            return RedirectToAction("Index", "Users");
        }

        List<PurchaseThemeViewModel> purchaseThemeViewModels = purchasedThemes
            .OrderBy(t=>t.MyTheme.ThemeNumber).Select(purchase => new PurchaseThemeViewModel
            {
                UserPhoneNumber = user?.PhoneNumber,
                ThemId = purchase.MyTheme?.Id ??
                         0, // Обратите внимание, что я использовал purchase.MyTheme?.Id и добавил ?? 0, чтобы избежать Nullable типа в ThemId
                ThemeNumber = purchase.MyTheme.ThemeNumber,
                ThemeName = purchase.MyTheme?.Name,
                AccessToDateTime = purchase.AccessTo // Если AccessTo Nullable DateTime?, преобразуйте его в строку
            })
            .ToList();


        return View(purchaseThemeViewModels);
    }


    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> PopularClickThems()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(currentUser, "block"))
        {
            return RedirectToAction("BlockPage", "Users");
        }

        if (string.IsNullOrEmpty(currentUser.Id))
        {
            return RedirectToAction("Register", "Account");
        }

        var themeInfo = _db.MyThemes.OrderByDescending(t => t.ClickCount)
            .Select(t => new { Id = t.Id, ThemeNumber = t.ThemeNumber, Name = t.Name, ClickCount = t.ClickCount })
            .ToList();

        List<PopularThemsViewModel> popularThems = new List<PopularThemsViewModel>();

        foreach (var theme in themeInfo)
        {
            var popularTheme = new PopularThemsViewModel
            {
                Id = theme.Id,
                ThemeNumber = theme.ThemeNumber,
                Name = theme.Name,
                ClickCount = theme.ClickCount
            };
            popularThems.Add(popularTheme);
        }

        return View(popularThems);
    }

    [HttpGet]
    [Authorize(Roles = "admin,superAdmin")]
    public async Task<IActionResult> PopularPurchaseThems()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(currentUser, "block"))
        {
            return RedirectToAction("BlockPage", "Users");
        }

        if (string.IsNullOrEmpty(currentUser.Id))
        {
            return RedirectToAction("Register", "Account");
        }

        var themeInfo = _db.MyThemes.OrderByDescending(t => t.RealPurchaseCount)
            .Select(t => new { Id = t.Id, Name = t.Name, RealPurchaseCount = t.RealPurchaseCount })
            .ToList();

        List<PopularThemsViewModel> popularThems = new List<PopularThemsViewModel>();

        foreach (var theme in themeInfo)
        {
            var popularTheme = new PopularThemsViewModel
            {
                Id = theme.Id,
                Name = theme.Name,
                RealPurchaseCount = theme.RealPurchaseCount
            };
            popularThems.Add(popularTheme);
        }

        return View(popularThems);
    }

    [HttpPost]
    public async Task<IActionResult> QuotesFromLuna(Quote quote)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(currentUser, "block"))
        {
            return RedirectToAction("BlockPage", "Users");
        }

        if (string.IsNullOrEmpty(currentUser.Id))
        {
            return RedirectToAction("Register", "Account");
        }

        if (string.IsNullOrEmpty(quote.Text))
        {
            return RedirectToAction("Index");
        }

        var quoteToUpdate = _db.Quotes.FirstOrDefault(q => q.Id == 1);
        if (quoteToUpdate != null)
        {
            quoteToUpdate.Text = quote.Text;
            _db.Quotes.Update(quoteToUpdate);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        _db.Quotes.Add(quote);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
}
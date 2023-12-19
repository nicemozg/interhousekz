using English_games.Context;
using English_games.Models;
using English_games.ViewModels.Main;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace English_games.Controllers;

public class MainController : Controller
{
    private English_gamesContext _db;
    private readonly UserManager<User> _userManager;

    public MainController(English_gamesContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var header = _db.MainContetntHeaders.FirstOrDefault(h => h.Id == 1);
        var contents = _db.MainContents.OrderBy(c => c.Id).ToList();
        MainViewModel mainViewModel = new MainViewModel();
        if (header != null)
        {
            mainViewModel.HeaderId = 1;
            mainViewModel.Header = header.HeaderName;
            mainViewModel.MainContents = new List<MainContent>();
        }
        else
        {
            mainViewModel.MainContents = new List<MainContent>();
        }

        if (contents.Count != 0)
        {
            foreach (var content in contents)
            {
                MainContent contentViewModel = new MainContent()
                {
                    Id = content.Id,
                    Header = content.Header,
                    DescriptionMainPage = content.DescriptionMainPage
                };

                mainViewModel.MainContents.Add(contentViewModel);
            }
        }

        return View(mainViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddHeader(MainViewModel model)
    {
        var header = _db.MainContetntHeaders.FirstOrDefault(h => h.Id == 1);
        if (header != null)
        {
            _db.Remove(header);
        }

        if (!string.IsNullOrEmpty(model.Header))
        {
            MainContetntHeader mainContetntHeader = new MainContetntHeader()
            {
                Id = 1,
                HeaderName = model.Header
            };
            _db.MainContetntHeaders.Add(mainContetntHeader);
        }
        else
        {
            MainContetntHeader mainContetntHeader = new MainContetntHeader()
            {
                Id = 1,
                HeaderName = "InterHouse"
            };
            _db.MainContetntHeaders.Add(mainContetntHeader);
        }

        _db.SaveChanges();
        return Redirect("Index");
    }

    [HttpPost]
    public IActionResult AddContent(MainViewModel model)
    {
        if (!string.IsNullOrEmpty(model.HeaderForDescription) && !string.IsNullOrEmpty(model.DescriptionMainPage))
        {
            MainContent mainContent = new MainContent()
            {
                Header = model.HeaderForDescription,
                DescriptionMainPage = model.DescriptionMainPage
            };
            _db.MainContents.Add(mainContent);
            _db.SaveChanges();
        }
       
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult EditContent(int contentId)
    {
        if (contentId <= 0)
        {
            return RedirectToAction("Index");
        }
        var content = _db.MainContents.FirstOrDefault(c => c.Id == contentId);
        MainViewModel mainViewModel = new MainViewModel();
        mainViewModel.ContentId = content.Id;
        mainViewModel.HeaderForDescription = content.Header;
        mainViewModel.DescriptionMainPage = content.DescriptionMainPage;
        return View(mainViewModel);
    }

    [HttpPost]
    public IActionResult EditContent(MainViewModel model)
    {
       
        if (!string.IsNullOrEmpty(model.HeaderForDescription) && !string.IsNullOrEmpty(model.DescriptionMainPage))
        {
            var content = _db.MainContents.FirstOrDefault(c => c.Id == model.ContentId);
            content.Header = model.HeaderForDescription;
            content.DescriptionMainPage = model.DescriptionMainPage;
            _db.MainContents.Update(content);
            _db.SaveChanges();
        }
        
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult DeleteHeader()
    {
        var header = _db.MainContetntHeaders.FirstOrDefault(h => h.Id == 1);
        if (header != null)
        {
            _db.MainContetntHeaders.Remove(header);
            _db.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult DeleteConntetnt(int contetntId)
    {
        var content = _db.MainContents.FirstOrDefault(c => c.Id == contetntId);
        _db.MainContents.Remove(content);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
}
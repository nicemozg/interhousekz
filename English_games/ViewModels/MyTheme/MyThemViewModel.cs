using System.ComponentModel.DataAnnotations;
using English_games.Enum;
using English_games.Models;

namespace English_games.ViewModels;

public class MyThemViewModel
{
    public int? Id { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
    public int ThemeNumber { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string LinkForVideo_16_Plus { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string LinkForVideo_16_Minus { get; set; }
    [Display(Name = "Фото для Preview")]
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public IFormFile PreviewFile { get; set; }
    public DateTime? AccessTo { get; set; }
    public DateTime CreatedAtMyTheme { get; set; }
    public string? LinkForPreview { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public int Price_3_Months { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public int Price_6_Months { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public int Price_Infinity { get; set; }
    
    public List<LinkForGame>? LinkForGames { get; set; }
    public List<LinkForBook>? LinkForBooks { get; set; }
    public List<Purchase>? Purchases { get; set; }
    
    public bool Purchase { get; set; }
    public int  ClickCount { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public int PurchaseCount { get; set; }
    public bool Adult { get; set; }
    public AddGameViewModel AddGameModel { get; set; }

    public MyThemViewModel()
    {
        AddGameModel = new AddGameViewModel();
    }
    

    public MyThemViewModel(MyTheme myTheme)
    {
        Id = myTheme.Id;
        Name = myTheme.Name;
        ThemeNumber = myTheme.ThemeNumber;
        LinkForVideo_16_Plus = myTheme.LinkForVideo_16_Plus;
        LinkForVideo_16_Minus = myTheme.LinkForVideo_16_Minus;
        LinkForPreview = myTheme.LinkForPreview;
        Price_3_Months = myTheme.Price_3_Months;
        Price_6_Months = myTheme.Price_6_Months;
        Price_Infinity = myTheme.Price_Infinity;
        ClickCount = myTheme.ClickCount;
        PurchaseCount = myTheme.FakePurchaseCount;
    }
}
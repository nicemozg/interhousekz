namespace English_games.ViewModels;

public class PurchaseThemeViewModel
{
    public string UserPhoneNumber { get; set; }
    public int ThemeNumber { get; set; }
    public int ThemId { get; set; }
    public string ThemeName { get; set; }
    public string AccessTo { get; set; }
    public DateTime? AccessToDateTime { get; set; }

    public PurchaseThemeViewModel()
    {
    }
}
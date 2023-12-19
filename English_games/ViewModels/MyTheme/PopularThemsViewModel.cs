using English_games.Models;

namespace English_games.ViewModels;

public class PopularThemsViewModel
{
    public int Id { get; set; }
    public int ThemeNumber { get; set; }
    public string Name { get; set; }
    public int  ClickCount { get; set; }
    public int RealPurchaseCount { get; set; }

    public PopularThemsViewModel()
    {
    }
}
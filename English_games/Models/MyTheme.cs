namespace English_games.Models;

public class MyTheme
{
    public int Id { get; set; }
    public int ThemeNumber { get; set; }
    public string? Name { get; set; }
    public string? LinkForVideo_16_Plus { get; set; }
    public string? LinkForVideo_16_Minus { get; set; }
    public string? LinkForPreview { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public int Price_3_Months { get; set; }
    public int Price_6_Months { get; set; }
    public int Price_Infinity { get; set; }
    public int  ClickCount { get; set; }
    public int FakePurchaseCount { get; set; }
    public int RealPurchaseCount { get; set; }
    public List<LinkForGame>? LinkForGames { get; set; }
    public List<LinkForBook>? LinkForBooks { get; set; }
    public IEnumerable<Purchase>? Purchases { get; set; }
    
    

    public MyTheme()
    {
    }
}
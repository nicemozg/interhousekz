namespace English_games.Models;

public class LinkForBook
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? LinkPathForBook { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int? MyThemeId { get; set; }
    public MyTheme MyTheme { get; set; }
}
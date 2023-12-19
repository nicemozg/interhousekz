using English_games.Enum;

namespace English_games.Models;

public class LinkForGame
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? LinkPathForGame { get; set; }
    public DateTime CreatedAt { get; set; }
    public Game Game { get; set; }
    public string? DescriptionForGame { get; set; }
    public int? MyThemeId { get; set; }
    public MyTheme MyTheme { get; set; }

    public LinkForGame()
    {
        
    }
}
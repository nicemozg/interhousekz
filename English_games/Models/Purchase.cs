namespace English_games.Models;

public class Purchase
{
    public int Id { get; set; }
    public DateTime? AccessTo { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
    public int? MyThemeId { get; set; }
    public MyTheme? MyTheme { get; set; }
}
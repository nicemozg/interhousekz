using Microsoft.AspNetCore.Identity;

namespace English_games.Models;

public class User : IdentityUser
{
    public int IntUserId { get; set; }
    public string? AvatarFileName { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime Creation { get; set; }
    public int Balance { get; set; }
    public string? CodeNumber { get; set; }
    public bool Block { get; set; }
    public bool Admin { get; set; }
    public bool Adult { get; set; }
    public ICollection<Purchase> Purchases { get; set; }
    public User()
    {
    }
}
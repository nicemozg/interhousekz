using System.ComponentModel.DataAnnotations;

namespace English_games.ViewModels.User;

public class UserViewModel
{
    public string Id { get; set; }
    public int IntUserId { get; set; }
    public string UserName { get; set; }
    public DateTime Creation { get; set; }
    public int Balance { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public int ReplenishmentAmount { get; set; }
    public bool Block { get; set; }
    public bool Admin { get; set; }
    public int PurchaseThemes { get; set; }

    public UserViewModel()
    {
    }
}
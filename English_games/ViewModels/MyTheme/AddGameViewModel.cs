using System.ComponentModel.DataAnnotations;
using English_games.Enum;
using English_games.Models;

namespace English_games.ViewModels;

public class AddGameViewModel
{
    public int Id { get; set; }
    public int ThemId { get; set; }
    public Game Game { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string? Name { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string DescriptionForGame { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string? LinkPathForGame { get; set; }

    public AddGameViewModel(LinkForGame linkForGame)
    {
        Id = linkForGame.Id;
        Game = linkForGame.Game;
        Name = linkForGame.Name;
        DescriptionForGame = linkForGame.DescriptionForGame;
        LinkPathForGame = linkForGame.LinkPathForGame;
    }

    public AddGameViewModel()
    {
    }
}
using System.ComponentModel.DataAnnotations;
using English_games.Models;

namespace English_games.ViewModels;

public class AddBookViewModel
{
    public int Id { get; set; }
    public int? ThemeId { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string? LinkPathForBook { get; set; }

    public AddBookViewModel()
    {
    }

    public AddBookViewModel(LinkForBook linkForBook)
    {
        Id = linkForBook.Id;
        ThemeId = linkForBook.MyThemeId;
        Name = linkForBook.Name;
        LinkPathForBook = linkForBook.LinkPathForBook;
    }
}
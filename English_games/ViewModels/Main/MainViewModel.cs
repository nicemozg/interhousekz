using System.ComponentModel.DataAnnotations;
using English_games.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace English_games.ViewModels.Main;

public class MainViewModel
{
    public int? HeaderId { get; set; }
    public int? ContentId { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string Header { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string HeaderForDescription { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string DescriptionMainPage { get; set; }
    public List<MainContent> MainContents { get; set; }


    public MainViewModel()
    {
    }

    public MainViewModel(MainContent mainContent)
    {
        ContentId = mainContent.Id;
        HeaderForDescription = mainContent.Header;
        DescriptionMainPage = mainContent.DescriptionMainPage;
    }
}
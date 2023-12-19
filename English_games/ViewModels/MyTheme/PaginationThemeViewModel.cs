using English_games.Models;

namespace English_games.ViewModels;

public class PaginationThemeViewModel
{
    public List<MyThemViewModel> Themes { get; set; }
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
namespace English_games.ViewModels.Account;

public class PasswordResetViewModel
{
    public string recipient { get; set; }
    public string text { get; set; }
    public string apiKey { get; set; }

    public PasswordResetViewModel()
    {
    }
}
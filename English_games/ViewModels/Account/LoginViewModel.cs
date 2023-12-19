using System.ComponentModel.DataAnnotations;

namespace English_games.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [Display(Name = "Номер телефона")]
    public string EmailOrUserName { get; set; }

    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
    [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]+$", ErrorMessage = "Пароль должен содержать только латинские буквы")]


    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }

    [Display(Name = "Запомнить")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
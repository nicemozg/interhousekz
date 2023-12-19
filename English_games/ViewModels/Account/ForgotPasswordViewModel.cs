using System.ComponentModel.DataAnnotations;

namespace English_games.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [Display(Name = "Номер")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Номер должен состоять из 10 цифр.")]
    public string UserName { get; set; }
}
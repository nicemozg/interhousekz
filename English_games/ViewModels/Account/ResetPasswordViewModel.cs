using System.ComponentModel.DataAnnotations;

namespace English_games.Models;

public class ResetPasswordViewModel
{
    
   
    public string? UserName { get; set; }
 
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Пароль должен содержать только латинские буквы и цифры.")]
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string Password { get; set; }
 
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Пароль должен содержать только латинские буквы и цифры.")]
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [DataType(DataType.Password)]
    [Display(Name = "Повторите пароль")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; }
    
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [RegularExpression("^[0-9]{4}$", ErrorMessage = "Код должен состоять из 4 цифр.")]
    [Display(Name = "Введите код из смс")]
    public string CodeNumberForResetPassword { get; set; }


    public string Code { get; set; }
}
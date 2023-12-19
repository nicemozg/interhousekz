using System.ComponentModel.DataAnnotations;
using English_games.Validation;

namespace English_games.ViewModels.Account;

public class RegisterViewModel
{
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Длина должна быть от 2 до 12 символов")]
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [Display(Name = "Имя")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    public string PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Длина должна быть от 2 до 20 символов")]
    [Display(Name = "Login")]
    [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*]+$", ErrorMessage = "Логин должен содержать только латинские буквы, цифры и символы: !@#$%^&*")]
    public string UserName { get; set; }
    
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [Display(Name = "Email")]
    [RegularExpression(@"^[\w\.-]+@[\w\.-]+\.\w+$", ErrorMessage = "НЕ валидный адрес электронной почты")]
    public string Email { get; set; }

   
    // [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Пожалуйста, выберите файл с расширением .jpg, .jpeg или .png")]
    [MaxFileSize(10 * 1024 * 1024, ErrorMessage = "Максимальный размер файла - 10 МБ")]
    [Display(Name = "Загрузить изображение")]
    public IFormFile? AvatarFile { get; set; }
    
    public string? AvatarFileName { get; set; }
    
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Пароль должен содержать только латинские буквы и цифры.")]
    [Display(Name = "Пароль")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Поле обязательно для заполнения")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтвердить пароль")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string PasswordConfirm { get; set; }
    public bool Adult { get; set; }
}
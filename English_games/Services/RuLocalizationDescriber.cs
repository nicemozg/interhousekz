using Microsoft.AspNetCore.Identity;

namespace English_games.Services;

public class RuLocalizationDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError()
        {
            Code = nameof(DuplicateEmail),
            Description = "Данный почтовый адрес уже зарегистрирован"
        };
    }
    
    public override IdentityError DuplicateUserName(string email)
    {
        return new IdentityError()
        {
            Code = nameof(DuplicateEmail),
            Description = "Имя пользователя занято"
        };
    }
    
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
    {
        return new IdentityError()
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = "Пароль должен содержать как минимум 2 уникальных символа"
        };
    }
}
using FluentValidation;
using Internal;

namespace quiz_web_app.Infrastructure.ValidationModels
{
    public class UserValidation : AbstractValidator<UserDto>
    {
        public UserValidation() 
        { 
                RuleFor(u => u.Password)
                .NotEmpty()
                .NotNull()
                .Length(7, 25)
                .WithMessage("Неверный формат пароля");

                RuleFor(u => u.Login)
                    .NotEmpty()
                    .NotNull()
                    .EmailAddress()
                    .WithMessage("Убедитесь, что Вы ввели email для поля логина");
        }
    }
}

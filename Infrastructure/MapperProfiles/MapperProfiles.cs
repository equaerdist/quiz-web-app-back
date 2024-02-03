using AutoMapper;
using Core.Models;
using Internal;
using quiz_web_app.Models;

namespace quiz_web_app.Infrastructure.MapperProfiles
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles() 
        {
            #region Пользовательские
            CreateMap<UserDto, User>();
            #endregion
            #region создание квиза
            CreateMap<CreateQuizCardDto, QuizCard>().ForMember(x => x.Questions, opt => opt.Ignore());
            CreateMap<CreateQuizDto, Quiz>().ForMember(x => x.Mode, opt => opt.Ignore());
            CreateMap<CreateQuizCardQuestionDto, QuizQuestion>();
            #endregion
            #region получение квизов
            CreateMap<Quiz, GetQuizDto>();
            #endregion
        }
    }
}

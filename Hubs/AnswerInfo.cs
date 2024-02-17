﻿using Internal;

namespace quiz_web_app.Hubs
{
    public class AnswerInfo
    {
        public List<GetQuestionDto> RightAnswers { get; set; } = null!;
        public int Award { get; set; }
        public TimeSpan Elapsed { get; set; } 
    }
}

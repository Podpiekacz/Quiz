using System.Collections.Generic;

namespace QuizApp.Models
{
    public class Question
    {
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        
        /// <summary>
        /// For SingleChoice and TrueFalse: index of the correct option (as string).
        /// For MultipleChoice: comma-separated indices of correct options (e.g. "0,2,3").
        /// For OpenEnded: the expected answer text.
        /// </summary>
        public string CorrectAnswer { get; set; } = string.Empty;

        /// <summary>
        /// Optional explanation shown after answering.
        /// </summary>
        public string Explanation { get; set; } = string.Empty;
    }
}

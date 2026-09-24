using System.Collections.Generic;

namespace QuizApp.Models
{
    public class QuizResult
    {
        public Quiz Quiz { get; set; } = null!;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public List<QuestionResult> QuestionResults { get; set; } = new List<QuestionResult>();
        
        public double ScorePercentage => TotalQuestions > 0 
            ? (double)CorrectAnswers / TotalQuestions * 100 
            : 0;
    }

    public class QuestionResult
    {
        public Question Question { get; set; } = null!;
        public string UserAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace QuizApp.Models
{
    public class Quiz
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string IconEmoji { get; set; } = "📝";
        public List<Question> Questions { get; set; } = new List<Question>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsBuiltIn { get; set; } = false;
    }
}

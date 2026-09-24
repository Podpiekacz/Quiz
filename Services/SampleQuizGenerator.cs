using System;
using System.Collections.Generic;
using QuizApp.Models;

namespace QuizApp.Services
{
    public static class SampleQuizGenerator
    {
        public static List<Quiz> CreateSampleQuizzes()
        {
            return new List<Quiz>
            {
                CreateGeneralKnowledgeQuiz(),
                CreateScienceQuiz(),
                CreateTechQuiz()
            };
        }

        private static Quiz CreateGeneralKnowledgeQuiz()
        {
            return new Quiz
            {
                Id = "sample-general-knowledge",
                Title = "General Knowledge",
                Description = "Test your general knowledge with a mix of questions from various topics!",
                Category = "General",
                IconEmoji = "🌍",
                IsBuiltIn = true,
                CreatedAt = new DateTime(2026, 1, 1),
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = "What is the capital of Australia?",
                        Type = QuestionType.SingleChoice,
                        Options = new List<string> { "Sydney", "Melbourne", "Canberra", "Brisbane" },
                        CorrectAnswer = "2",
                        Explanation = "Canberra is the capital city of Australia, chosen as a compromise between Sydney and Melbourne."
                    },
                    new Question
                    {
                        Text = "The Great Wall of China is visible from space with the naked eye.",
                        Type = QuestionType.TrueFalse,
                        Options = new List<string> { "True", "False" },
                        CorrectAnswer = "1",
                        Explanation = "This is a common myth. The Great Wall is not visible from space with the naked eye according to astronauts."
                    },
                    new Question
                    {
                        Text = "Which of the following are official languages of the United Nations?",
                        Type = QuestionType.MultipleChoice,
                        Options = new List<string> { "English", "German", "Arabic", "Chinese", "Japanese", "French" },
                        CorrectAnswer = "0,2,3,5",
                        Explanation = "The six official UN languages are: Arabic, Chinese, English, French, Russian, and Spanish."
                    },
                    new Question
                    {
                        Text = "Who wrote the play 'Romeo and Juliet'?",
                        Type = QuestionType.OpenEnded,
                        Options = new List<string>(),
                        CorrectAnswer = "William Shakespeare",
                        Explanation = "William Shakespeare wrote Romeo and Juliet around 1594-1596."
                    },
                    new Question
                    {
                        Text = "Which planet is known as the Red Planet?",
                        Type = QuestionType.SingleChoice,
                        Options = new List<string> { "Venus", "Mars", "Jupiter", "Saturn" },
                        CorrectAnswer = "1",
                        Explanation = "Mars is called the Red Planet due to iron oxide (rust) on its surface."
                    },
                    new Question
                    {
                        Text = "The Mona Lisa was painted by Leonardo da Vinci.",
                        Type = QuestionType.TrueFalse,
                        Options = new List<string> { "True", "False" },
                        CorrectAnswer = "0",
                        Explanation = "Yes, the Mona Lisa (La Gioconda) was painted by Leonardo da Vinci, likely between 1503 and 1519."
                    },
                    new Question
                    {
                        Text = "What is the largest ocean on Earth?",
                        Type = QuestionType.SingleChoice,
                        Options = new List<string> { "Atlantic Ocean", "Indian Ocean", "Arctic Ocean", "Pacific Ocean" },
                        CorrectAnswer = "3",
                        Explanation = "The Pacific Ocean is the largest and deepest ocean, covering about one-third of Earth's surface."
                    },
                    new Question
                    {
                        Text = "Name the author of 'A Brief History of Time'.",
                        Type = QuestionType.OpenEnded,
                        Options = new List<string>(),
                        CorrectAnswer = "Stephen Hawking",
                        Explanation = "Stephen Hawking published 'A Brief History of Time' in 1988."
                    }
                }
            };
        }

        private static Quiz CreateScienceQuiz()
        {
            return new Quiz
            {
                Id = "sample-science",
                Title = "Science & Nature",
                Description = "Explore the wonders of science with questions about physics, chemistry, and biology!",
                Category = "Science",
                IconEmoji = "🔬",
                IsBuiltIn = true,
                CreatedAt = new DateTime(2026, 1, 1),
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = "What is the chemical symbol for gold?",
                        Type = QuestionType.SingleChoice,
                        Options = new List<string> { "Go", "Gd", "Au", "Ag" },
                        CorrectAnswer = "2",
                        Explanation = "Au comes from the Latin word 'aurum', meaning gold."
                    },
                    new Question
                    {
                        Text = "Which of the following are noble gases?",
                        Type = QuestionType.MultipleChoice,
                        Options = new List<string> { "Helium", "Oxygen", "Neon", "Nitrogen", "Argon", "Krypton" },
                        CorrectAnswer = "0,2,4,5",
                        Explanation = "Noble gases include Helium, Neon, Argon, Krypton, Xenon, and Radon."
                    },
                    new Question
                    {
                        Text = "Water boils at 100°C at sea level.",
                        Type = QuestionType.TrueFalse,
                        Options = new List<string> { "True", "False" },
                        CorrectAnswer = "0",
                        Explanation = "At standard atmospheric pressure (1 atm), water boils at exactly 100°C (212°F)."
                    },
                    new Question
                    {
                        Text = "What is the powerhouse of the cell?",
                        Type = QuestionType.OpenEnded,
                        Options = new List<string>(),
                        CorrectAnswer = "Mitochondria",
                        Explanation = "Mitochondria are often called the powerhouse of the cell because they generate most of the cell's ATP."
                    },
                    new Question
                    {
                        Text = "What is the speed of light in a vacuum (approximately in km/s)?",
                        Type = QuestionType.SingleChoice,
                        Options = new List<string> { "150,000 km/s", "300,000 km/s", "450,000 km/s", "600,000 km/s" },
                        CorrectAnswer = "1",
                        Explanation = "The speed of light in a vacuum is approximately 299,792 km/s, roughly 300,000 km/s."
                    },
                    new Question
                    {
                        Text = "Diamonds are made entirely of carbon atoms.",
                        Type = QuestionType.TrueFalse,
                        Options = new List<string> { "True", "False" },
                        CorrectAnswer = "0",
                        Explanation = "Diamonds are composed of pure carbon atoms arranged in a crystal lattice structure."
                    },
                    new Question
                    {
                        Text = "Which of these animals are mammals?",
                        Type = QuestionType.MultipleChoice,
                        Options = new List<string> { "Dolphin", "Shark", "Whale", "Salmon", "Bat" },
                        CorrectAnswer = "0,2,4",
                        Explanation = "Dolphins, whales, and bats are mammals. Sharks and salmon are fish."
                    },
                    new Question
                    {
                        Text = "What planet has the most moons in our solar system?",
                        Type = QuestionType.SingleChoice,
                        Options = new List<string> { "Jupiter", "Saturn", "Uranus", "Neptune" },
                        CorrectAnswer = "1",
                        Explanation = "Saturn has over 140 confirmed moons, making it the planet with the most known moons."
                    }
                }
            };
        }

        private static Quiz CreateTechQuiz()
        {
            return new Quiz
            {
                Id = "sample-technology",
                Title = "Technology & Computing",
                Description = "How well do you know the world of technology? Find out with this quiz!",
                Category = "Technology",
                IconEmoji = "💻",
                IsBuiltIn = true,
                CreatedAt = new DateTime(2026, 1, 1),
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = "What does 'HTML' stand for?",
                        Type = QuestionType.SingleChoice,
                        Options = new List<string>
                        {
                            "Hyper Text Markup Language",
                            "High Tech Modern Language",
                            "Hyper Transfer Markup Language",
                            "Home Tool Markup Language"
                        },
                        CorrectAnswer = "0",
                        Explanation = "HTML stands for HyperText Markup Language, the standard markup language for web pages."
                    },
                    new Question
                    {
                        Text = "Which of the following are programming languages?",
                        Type = QuestionType.MultipleChoice,
                        Options = new List<string> { "Python", "Cobra", "Java", "Photoshop", "C#", "Excel" },
                        CorrectAnswer = "0,2,4",
                        Explanation = "Python, Java, and C# are programming languages. Cobra is also a real language, but here it's a distractor. Photoshop and Excel are applications."
                    },
                    new Question
                    {
                        Text = "The first computer virus was created in the 1980s.",
                        Type = QuestionType.TrueFalse,
                        Options = new List<string> { "True", "False" },
                        CorrectAnswer = "0",
                        Explanation = "'Brain' is considered the first IBM PC virus, created in 1986 by two Pakistani brothers."
                    },
                    new Question
                    {
                        Text = "What does 'CPU' stand for?",
                        Type = QuestionType.OpenEnded,
                        Options = new List<string>(),
                        CorrectAnswer = "Central Processing Unit",
                        Explanation = "CPU stands for Central Processing Unit, the primary component that executes instructions."
                    },
                    new Question
                    {
                        Text = "Who is considered the father of computer science?",
                        Type = QuestionType.SingleChoice,
                        Options = new List<string>
                        {
                            "Bill Gates",
                            "Alan Turing",
                            "Steve Jobs",
                            "Tim Berners-Lee"
                        },
                        CorrectAnswer = "1",
                        Explanation = "Alan Turing is widely considered the father of computer science and artificial intelligence."
                    },
                    new Question
                    {
                        Text = "Linux is an open-source operating system.",
                        Type = QuestionType.TrueFalse,
                        Options = new List<string> { "True", "False" },
                        CorrectAnswer = "0",
                        Explanation = "Linux is indeed open source, released under the GNU General Public License."
                    },
                    new Question
                    {
                        Text = "What year was the first iPhone released?",
                        Type = QuestionType.SingleChoice,
                        Options = new List<string> { "2005", "2006", "2007", "2008" },
                        CorrectAnswer = "2",
                        Explanation = "The first iPhone was released on June 29, 2007."
                    },
                    new Question
                    {
                        Text = "Which of these are types of databases?",
                        Type = QuestionType.MultipleChoice,
                        Options = new List<string> { "Relational", "Graph", "Blockchain", "NoSQL", "Vector" },
                        CorrectAnswer = "0,1,3,4",
                        Explanation = "Relational, Graph, NoSQL, and Vector are all types of databases. Blockchain is a different technology."
                    }
                }
            };
        }
    }
}

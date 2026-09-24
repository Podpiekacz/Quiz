using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using QuizApp.Models;

namespace QuizApp.Services
{
    public class QuizStorageService
    {
        private readonly string _storagePath;
        private const string FileName = "quizzes.json";

        public QuizStorageService()
        {
            _storagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "QuizApp");

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public string FilePath => Path.Combine(_storagePath, FileName);

        public List<Quiz> LoadQuizzes()
        {
            var filePath = FilePath;

            if (!File.Exists(filePath))
            {
                return new List<Quiz>();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var quizzes = JsonConvert.DeserializeObject<List<Quiz>>(json);
                return quizzes ?? new List<Quiz>();
            }
            catch (Exception)
            {
                return new List<Quiz>();
            }
        }

        public void SaveQuizzes(List<Quiz> quizzes)
        {
            var filePath = FilePath;
            var json = JsonConvert.SerializeObject(quizzes, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void AddQuiz(Quiz quiz)
        {
            var quizzes = LoadQuizzes();
            quizzes.Add(quiz);
            SaveQuizzes(quizzes);
        }

        public void UpdateQuiz(Quiz quiz)
        {
            var quizzes = LoadQuizzes();
            var index = quizzes.FindIndex(q => q.Id == quiz.Id);
            if (index >= 0)
            {
                quizzes[index] = quiz;
                SaveQuizzes(quizzes);
            }
        }

        public void DeleteQuiz(string quizId)
        {
            var quizzes = LoadQuizzes();
            quizzes.RemoveAll(q => q.Id == quizId);
            SaveQuizzes(quizzes);
        }
    }
}

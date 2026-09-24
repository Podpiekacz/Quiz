using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using QuizApp.Models;
using QuizApp.Services;

namespace QuizApp
{
    public partial class MainWindow : Window
    {
        private readonly QuizStorageService _storageService;
        private List<Quiz> _quizzes = new();

        // Quiz taking state
        private Quiz? _currentQuiz;
        private int _currentQuestionIndex;
        private List<QuestionResult> _questionResults = new();

        // Creator state
        private List<CreatorQuestion> _creatorQuestions = new();

        public MainWindow()
        {
            InitializeComponent();
            _storageService = new QuizStorageService();
            LoadQuizzes();
        }

        // ====================================================================
        // QUIZ LOADING & HOME VIEW
        // ====================================================================

        private void LoadQuizzes()
        {
            _quizzes = _storageService.LoadQuizzes();

            // If no quizzes exist, seed with sample data
            if (_quizzes.Count == 0)
            {
                _quizzes = SampleQuizGenerator.CreateSampleQuizzes();
                _storageService.SaveQuizzes(_quizzes);
            }

            RenderQuizCards();
        }

        private void RenderQuizCards()
        {
            QuizList.Items.Clear();

            foreach (var quiz in _quizzes)
            {
                var card = CreateQuizCard(quiz);
                QuizList.Items.Add(card);
            }
        }

        private Border CreateQuizCard(Quiz quiz)
        {
            var card = new Border
            {
                Width = 300,
                Margin = new Thickness(0, 0, 20, 20),
                CornerRadius = new CornerRadius(16),
                BorderThickness = new Thickness(1),
                BorderBrush = (SolidColorBrush)FindResource("BorderBrush"),
                Background = (SolidColorBrush)FindResource("BgCardBrush"),
                Cursor = Cursors.Hand,
                Padding = new Thickness(24),
                Tag = quiz.Id
            };

            // Hover effects
            card.MouseEnter += (s, e) =>
            {
                card.Background = (SolidColorBrush)FindResource("BgCardHoverBrush");
                card.BorderBrush = (SolidColorBrush)FindResource("AccentPrimaryBrush");
            };
            card.MouseLeave += (s, e) =>
            {
                card.Background = (SolidColorBrush)FindResource("BgCardBrush");
                card.BorderBrush = (SolidColorBrush)FindResource("BorderBrush");
            };
            card.MouseLeftButtonUp += (s, e) => StartQuiz(quiz);

            var stack = new StackPanel();

            // Icon & category row
            var topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var emoji = new TextBlock
            {
                Text = quiz.IconEmoji,
                FontSize = 36,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(emoji, 0);
            topRow.Children.Add(emoji);

            // Delete button (only for non-built-in quizzes, or allow deleting all)
            if (!quiz.IsBuiltIn)
            {
                var deleteBtn = new Button
                {
                    Content = "🗑",
                    FontSize = 16,
                    Style = (Style)FindResource("GhostButton"),
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(6, 4, 6, 4),
                    Tag = quiz.Id
                };
                deleteBtn.Click += DeleteQuiz_Click;
                Grid.SetColumn(deleteBtn, 2);
                topRow.Children.Add(deleteBtn);
            }

            stack.Children.Add(topRow);

            // Category badge
            var catBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(25, 108, 99, 255)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 12, 0, 0)
            };
            catBorder.Child = new TextBlock
            {
                Text = quiz.Category,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)FindResource("AccentPrimaryBrush")
            };
            stack.Children.Add(catBorder);

            // Title
            stack.Children.Add(new TextBlock
            {
                Text = quiz.Title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush"),
                Margin = new Thickness(0, 10, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });

            // Description
            stack.Children.Add(new TextBlock
            {
                Text = quiz.Description,
                FontSize = 13,
                Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                Margin = new Thickness(0, 6, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 50,
                TextTrimming = TextTrimming.CharacterEllipsis
            });

            // Footer info
            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 14, 0, 0)
            };

            // Question count
            var qCountBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(15, 255, 255, 255)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(0, 0, 8, 0)
            };
            qCountBorder.Child = new TextBlock
            {
                Text = $"📋 {quiz.Questions.Count} questions",
                FontSize = 12,
                Foreground = (SolidColorBrush)FindResource("TextMutedBrush")
            };
            footer.Children.Add(qCountBorder);

            // Question types count
            var types = quiz.Questions.Select(q => q.Type).Distinct().Count();
            var typesBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(15, 255, 255, 255)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 4, 8, 4)
            };
            typesBorder.Child = new TextBlock
            {
                Text = $"🎯 {types} types",
                FontSize = 12,
                Foreground = (SolidColorBrush)FindResource("TextMutedBrush")
            };
            footer.Children.Add(typesBorder);

            stack.Children.Add(footer);
            card.Child = stack;

            return card;
        }

        // ====================================================================
        // QUIZ TAKING
        // ====================================================================

        private void StartQuiz(Quiz quiz)
        {
            _currentQuiz = quiz;
            _currentQuestionIndex = 0;
            _questionResults = new List<QuestionResult>();

            QuizTitleText.Text = quiz.Title;
            ShowView("QuizTaking");
            DisplayCurrentQuestion();
        }

        private void DisplayCurrentQuestion()
        {
            if (_currentQuiz == null) return;

            var question = _currentQuiz.Questions[_currentQuestionIndex];
            var total = _currentQuiz.Questions.Count;

            // Update counter & progress
            QuestionCounter.Text = $"Question {_currentQuestionIndex + 1} of {total}";
            double progress = (double)(_currentQuestionIndex) / total;
            ProgressBarFill.Width = progress * (ActualWidth - 80); // subtract padding

            // Question type badge
            QuestionTypeText.Text = question.Type switch
            {
                QuestionType.SingleChoice => "📌 Single Choice",
                QuestionType.MultipleChoice => "☑️ Multiple Choice — select all that apply",
                QuestionType.OpenEnded => "✍️ Open Ended",
                QuestionType.TrueFalse => "⚡ True or False",
                _ => "Question"
            };

            QuestionText.Text = question.Text;

            // Reset feedback
            FeedbackPanel.Visibility = Visibility.Collapsed;
            SubmitAnswerBtn.Visibility = Visibility.Visible;
            NextQuestionBtn.Visibility = Visibility.Collapsed;

            // Build answer controls
            AnswerPanel.Children.Clear();

            switch (question.Type)
            {
                case QuestionType.SingleChoice:
                    RenderSingleChoiceOptions(question);
                    break;
                case QuestionType.MultipleChoice:
                    RenderMultipleChoiceOptions(question);
                    break;
                case QuestionType.TrueFalse:
                    RenderTrueFalseOptions(question);
                    break;
                case QuestionType.OpenEnded:
                    RenderOpenEndedInput();
                    break;
            }

            // Play fade animation
            var storyboard = (Storyboard)FindResource("FadeIn");
            QuestionArea.BeginStoryboard(storyboard);
        }

        private void RenderSingleChoiceOptions(Question question)
        {
            for (int i = 0; i < question.Options.Count; i++)
            {
                var rb = new RadioButton
                {
                    Content = question.Options[i],
                    GroupName = "QuizAnswer",
                    Tag = i,
                    Style = (Style)FindResource("ModernRadioButton"),
                    FontSize = 15
                };
                AnswerPanel.Children.Add(rb);
            }
        }

        private void RenderMultipleChoiceOptions(Question question)
        {
            for (int i = 0; i < question.Options.Count; i++)
            {
                var cb = new CheckBox
                {
                    Content = question.Options[i],
                    Tag = i,
                    Style = (Style)FindResource("ModernCheckBox"),
                    FontSize = 15
                };
                AnswerPanel.Children.Add(cb);
            }
        }

        private void RenderTrueFalseOptions(Question question)
        {
            var trueBtn = new RadioButton
            {
                Content = "True",
                GroupName = "QuizAnswer",
                Tag = 0,
                Style = (Style)FindResource("ModernRadioButton"),
                FontSize = 15
            };
            var falseBtn = new RadioButton
            {
                Content = "False",
                GroupName = "QuizAnswer",
                Tag = 1,
                Style = (Style)FindResource("ModernRadioButton"),
                FontSize = 15
            };
            AnswerPanel.Children.Add(trueBtn);
            AnswerPanel.Children.Add(falseBtn);
        }

        private void RenderOpenEndedInput()
        {
            var textBox = new TextBox
            {
                Style = (Style)FindResource("ModernTextBox"),
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = false,
                Height = 50,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            // Placeholder text
            var placeholder = new TextBlock
            {
                Text = "Type your answer here...",
                FontSize = 15,
                Foreground = (SolidColorBrush)FindResource("TextMutedBrush"),
                IsHitTestVisible = false,
                Margin = new Thickness(16, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var grid = new Grid { Margin = new Thickness(0, 5, 0, 0) };
            grid.Children.Add(textBox);
            grid.Children.Add(placeholder);

            textBox.TextChanged += (s, e) =>
            {
                placeholder.Visibility = string.IsNullOrEmpty(textBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            };

            AnswerPanel.Children.Add(grid);
        }

        private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuiz == null) return;
            var question = _currentQuiz.Questions[_currentQuestionIndex];

            string userAnswer = "";
            bool isCorrect = false;

            switch (question.Type)
            {
                case QuestionType.SingleChoice:
                case QuestionType.TrueFalse:
                    var selectedRadio = AnswerPanel.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.IsChecked == true);
                    if (selectedRadio == null)
                    {
                        ShowToast("Please select an answer!");
                        return;
                    }
                    userAnswer = selectedRadio.Tag.ToString()!;
                    isCorrect = userAnswer == question.CorrectAnswer;
                    break;

                case QuestionType.MultipleChoice:
                    var selectedCheckboxes = AnswerPanel.Children.OfType<CheckBox>()
                        .Where(cb => cb.IsChecked == true)
                        .Select(cb => cb.Tag.ToString())
                        .ToList();
                    if (selectedCheckboxes.Count == 0)
                    {
                        ShowToast("Please select at least one answer!");
                        return;
                    }
                    userAnswer = string.Join(",", selectedCheckboxes);
                    var correctSet = new HashSet<string>(question.CorrectAnswer.Split(','));
                    var userSet = new HashSet<string>(selectedCheckboxes!);
                    isCorrect = correctSet.SetEquals(userSet);
                    break;

                case QuestionType.OpenEnded:
                    var textBox = FindTextBoxInAnswerPanel();
                    if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        ShowToast("Please type an answer!");
                        return;
                    }
                    userAnswer = textBox.Text.Trim();
                    isCorrect = string.Equals(userAnswer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            // Store result
            _questionResults.Add(new QuestionResult
            {
                Question = question,
                UserAnswer = userAnswer,
                IsCorrect = isCorrect
            });

            // Show feedback
            ShowFeedback(isCorrect, question);

            // Disable answer controls
            DisableAnswerControls();

            // Show next or finish button
            SubmitAnswerBtn.Visibility = Visibility.Collapsed;
            if (_currentQuestionIndex < _currentQuiz.Questions.Count - 1)
            {
                NextQuestionBtn.Content = "Next Question →";
            }
            else
            {
                NextQuestionBtn.Content = "See Results 🏆";
            }
            NextQuestionBtn.Visibility = Visibility.Visible;
        }

        private TextBox? FindTextBoxInAnswerPanel()
        {
            foreach (var child in AnswerPanel.Children)
            {
                if (child is Grid grid)
                {
                    foreach (var gridChild in grid.Children)
                    {
                        if (gridChild is TextBox tb) return tb;
                    }
                }
                if (child is TextBox textBox) return textBox;
            }
            return null;
        }

        private void ShowFeedback(bool isCorrect, Question question)
        {
            FeedbackPanel.Visibility = Visibility.Visible;

            if (isCorrect)
            {
                FeedbackPanel.Background = new SolidColorBrush(Color.FromArgb(25, 67, 233, 123));
                FeedbackPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(60, 67, 233, 123));
                FeedbackPanel.BorderThickness = new Thickness(1);
                FeedbackIcon.Text = "✅ Correct!";
                FeedbackText.Text = "Great job! You got it right.";
                FeedbackText.Foreground = (SolidColorBrush)FindResource("AccentTertiaryBrush");
            }
            else
            {
                FeedbackPanel.Background = new SolidColorBrush(Color.FromArgb(25, 255, 101, 132));
                FeedbackPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(60, 255, 101, 132));
                FeedbackPanel.BorderThickness = new Thickness(1);
                FeedbackIcon.Text = "❌ Incorrect";

                // Show the correct answer
                string correctText = GetCorrectAnswerText(question);
                FeedbackText.Text = $"The correct answer: {correctText}";
                FeedbackText.Foreground = (SolidColorBrush)FindResource("AccentSecondaryBrush");
            }

            if (!string.IsNullOrEmpty(question.Explanation))
            {
                ExplanationText.Visibility = Visibility.Visible;
                ExplanationText.Text = $"💡 {question.Explanation}";
            }
            else
            {
                ExplanationText.Visibility = Visibility.Collapsed;
            }

            var storyboard = (Storyboard)FindResource("FadeIn");
            FeedbackPanel.BeginStoryboard(storyboard);
        }

        private string GetCorrectAnswerText(Question question)
        {
            switch (question.Type)
            {
                case QuestionType.SingleChoice:
                    int idx = int.Parse(question.CorrectAnswer);
                    return question.Options[idx];

                case QuestionType.MultipleChoice:
                    var indices = question.CorrectAnswer.Split(',').Select(int.Parse);
                    return string.Join(", ", indices.Select(i => question.Options[i]));

                case QuestionType.TrueFalse:
                    return question.CorrectAnswer == "0" ? "True" : "False";

                case QuestionType.OpenEnded:
                    return question.CorrectAnswer;

                default:
                    return question.CorrectAnswer;
            }
        }

        private void DisableAnswerControls()
        {
            foreach (var child in AnswerPanel.Children)
            {
                if (child is RadioButton rb) rb.IsEnabled = false;
                if (child is CheckBox cb) cb.IsEnabled = false;
                if (child is Grid grid)
                {
                    foreach (var gc in grid.Children)
                    {
                        if (gc is TextBox tb) tb.IsEnabled = false;
                    }
                }
            }
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuiz == null) return;

            _currentQuestionIndex++;

            if (_currentQuestionIndex >= _currentQuiz.Questions.Count)
            {
                ShowResults();
            }
            else
            {
                DisplayCurrentQuestion();
            }
        }

        // ====================================================================
        // RESULTS VIEW
        // ====================================================================

        private void ShowResults()
        {
            if (_currentQuiz == null) return;

            var result = new QuizResult
            {
                Quiz = _currentQuiz,
                TotalQuestions = _currentQuiz.Questions.Count,
                CorrectAnswers = _questionResults.Count(r => r.IsCorrect),
                QuestionResults = _questionResults
            };

            ShowView("Results");

            // Score display
            double pct = result.ScorePercentage;
            ScorePercentText.Text = $"{pct:F0}%";
            ScoreDetailText.Text = $"{result.CorrectAnswers} / {result.TotalQuestions} correct";

            // Update progress on the fill bar (just update the width on next layout)
            double progressFraction = pct / 100.0;
            ProgressBarFill.Width = progressFraction * (ActualWidth - 80);

            // Score circle stroke
            if (pct >= 80)
            {
                ScoreCircle.Stroke = (Brush)FindResource("AccentTertiaryBrush");
                ResultEmoji.Text = "🏆";
                ResultTitle.Text = "Excellent!";
                ResultSubtitle.Text = "Outstanding performance! You're a quiz master!";
            }
            else if (pct >= 60)
            {
                ScoreCircle.Stroke = (Brush)FindResource("AccentInfoBrush");
                ResultEmoji.Text = "👏";
                ResultTitle.Text = "Good Job!";
                ResultSubtitle.Text = "You did well! Keep learning and improving!";
            }
            else if (pct >= 40)
            {
                ScoreCircle.Stroke = (Brush)FindResource("AccentWarningBrush");
                ResultEmoji.Text = "💪";
                ResultTitle.Text = "Not Bad!";
                ResultSubtitle.Text = "There's room for improvement. Try again!";
            }
            else
            {
                ScoreCircle.Stroke = (Brush)FindResource("AccentSecondaryBrush");
                ResultEmoji.Text = "📚";
                ResultTitle.Text = "Keep Studying!";
                ResultSubtitle.Text = "Don't give up! Practice makes perfect.";
            }

            // Set the stroke dash for circular progress
            double circumference = Math.PI * 150; // diameter * pi
            ScoreCircle.StrokeDashArray = new DoubleCollection { circumference * progressFraction / 8, circumference };

            // Build review panel
            ReviewPanel.Children.Clear();
            for (int i = 0; i < result.QuestionResults.Count; i++)
            {
                var qr = result.QuestionResults[i];
                var reviewCard = CreateReviewCard(i + 1, qr);
                ReviewPanel.Children.Add(reviewCard);
            }
        }

        private Border CreateReviewCard(int number, QuestionResult qr)
        {
            var card = new Border
            {
                Background = (SolidColorBrush)FindResource("BgCardBrush"),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 10),
                BorderThickness = new Thickness(1),
                BorderBrush = qr.IsCorrect
                    ? new SolidColorBrush(Color.FromArgb(60, 67, 233, 123))
                    : new SolidColorBrush(Color.FromArgb(60, 255, 101, 132))
            };

            var stack = new StackPanel();

            // Header row
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var qText = new TextBlock
            {
                Text = $"Q{number}. {qr.Question.Text}",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush"),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(qText, 0);
            header.Children.Add(qText);

            var statusIcon = new TextBlock
            {
                Text = qr.IsCorrect ? "✅" : "❌",
                FontSize = 18,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(statusIcon, 1);
            header.Children.Add(statusIcon);

            stack.Children.Add(header);

            // User answer
            string userAnswerText = GetUserAnswerText(qr);
            stack.Children.Add(new TextBlock
            {
                Text = $"Your answer: {userAnswerText}",
                FontSize = 13,
                Foreground = qr.IsCorrect
                    ? (SolidColorBrush)FindResource("AccentTertiaryBrush")
                    : (SolidColorBrush)FindResource("AccentSecondaryBrush"),
                Margin = new Thickness(0, 8, 0, 0)
            });

            if (!qr.IsCorrect)
            {
                string correctText = GetCorrectAnswerText(qr.Question);
                stack.Children.Add(new TextBlock
                {
                    Text = $"Correct answer: {correctText}",
                    FontSize = 13,
                    Foreground = (SolidColorBrush)FindResource("AccentTertiaryBrush"),
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            card.Child = stack;
            return card;
        }

        private string GetUserAnswerText(QuestionResult qr)
        {
            var q = qr.Question;
            switch (q.Type)
            {
                case QuestionType.SingleChoice:
                    if (int.TryParse(qr.UserAnswer, out int sIdx) && sIdx >= 0 && sIdx < q.Options.Count)
                        return q.Options[sIdx];
                    return qr.UserAnswer;

                case QuestionType.MultipleChoice:
                    var indices = qr.UserAnswer.Split(',')
                        .Where(s => int.TryParse(s, out _))
                        .Select(s => int.Parse(s))
                        .Where(i => i >= 0 && i < q.Options.Count)
                        .Select(i => q.Options[i]);
                    return string.Join(", ", indices);

                case QuestionType.TrueFalse:
                    return qr.UserAnswer == "0" ? "True" : "False";

                case QuestionType.OpenEnded:
                    return qr.UserAnswer;

                default:
                    return qr.UserAnswer;
            }
        }

        private void RetryQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuiz != null)
            {
                StartQuiz(_currentQuiz);
            }
        }

        // ====================================================================
        // QUIZ CREATOR
        // ====================================================================

        private void CreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            _creatorQuestions = new List<CreatorQuestion>();
            CreatorTitle.Text = "";
            CreatorDescription.Text = "";
            CreatorCategory.Text = "";
            CreatorEmoji.Text = "📝";
            CreatorQuestionList.Children.Clear();
            UpdateQuestionCountLabel();

            ShowView("Creator");

            // Add one default question
            AddNewCreatorQuestion();
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            AddNewCreatorQuestion();
        }

        private void AddNewCreatorQuestion()
        {
            var cq = new CreatorQuestion { Index = _creatorQuestions.Count };
            _creatorQuestions.Add(cq);

            var panel = CreateCreatorQuestionPanel(cq);
            CreatorQuestionList.Children.Add(panel);
            UpdateQuestionCountLabel();
        }

        private void UpdateQuestionCountLabel()
        {
            QuestionCountLabel.Text = $"{_creatorQuestions.Count} question(s)";
        }

        private Border CreateCreatorQuestionPanel(CreatorQuestion cq)
        {
            var card = new Border
            {
                Background = (SolidColorBrush)FindResource("BgCardBrush"),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(24),
                Margin = new Thickness(0, 0, 0, 14),
                BorderThickness = new Thickness(1),
                BorderBrush = (SolidColorBrush)FindResource("BorderBrush"),
                Tag = cq
            };

            var stack = new StackPanel();

            // Header with number and remove button
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var headerText = new TextBlock
            {
                Text = $"Question {cq.Index + 1}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(headerText, 0);
            header.Children.Add(headerText);

            if (_creatorQuestions.Count > 1 || true) // always show remove
            {
                var removeBtn = new Button
                {
                    Content = "✕ Remove",
                    Style = (Style)FindResource("GhostButton"),
                    Tag = cq,
                    Foreground = (SolidColorBrush)FindResource("AccentSecondaryBrush"),
                    FontSize = 12
                };
                removeBtn.Click += RemoveQuestion_Click;
                Grid.SetColumn(removeBtn, 1);
                header.Children.Add(removeBtn);
            }

            stack.Children.Add(header);

            // Question type selector
            stack.Children.Add(new TextBlock
            {
                Text = "Question Type",
                FontSize = 13,
                Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                Margin = new Thickness(0, 14, 0, 6)
            });

            var typeCombo = new ComboBox
            {
                Style = (Style)FindResource("ModernComboBox"),
                Tag = cq
            };
            typeCombo.Items.Add(new ComboBoxItem { Content = "Single Choice", Tag = QuestionType.SingleChoice });
            typeCombo.Items.Add(new ComboBoxItem { Content = "Multiple Choice", Tag = QuestionType.MultipleChoice });
            typeCombo.Items.Add(new ComboBoxItem { Content = "Open Ended", Tag = QuestionType.OpenEnded });
            typeCombo.Items.Add(new ComboBoxItem { Content = "True or False", Tag = QuestionType.TrueFalse });
            typeCombo.SelectedIndex = 0;
            cq.TypeCombo = typeCombo;
            stack.Children.Add(typeCombo);

            // Question text
            stack.Children.Add(new TextBlock
            {
                Text = "Question Text",
                FontSize = 13,
                Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                Margin = new Thickness(0, 14, 0, 6)
            });

            var questionTextBox = new TextBox
            {
                Style = (Style)FindResource("ModernTextBox"),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = false
            };
            cq.QuestionTextBox = questionTextBox;
            stack.Children.Add(questionTextBox);

            // Options area (dynamic based on type)
            var optionsPanel = new StackPanel { Tag = "OptionsPanel" };
            cq.OptionsPanel = optionsPanel;
            stack.Children.Add(optionsPanel);

            // Correct answer area
            var answerPanel = new StackPanel { Tag = "AnswerPanel" };
            cq.AnswerPanel = answerPanel;
            stack.Children.Add(answerPanel);

            // Explanation
            stack.Children.Add(new TextBlock
            {
                Text = "Explanation (optional)",
                FontSize = 13,
                Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                Margin = new Thickness(0, 14, 0, 6)
            });

            var explanationBox = new TextBox
            {
                Style = (Style)FindResource("ModernTextBox"),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 50
            };
            cq.ExplanationTextBox = explanationBox;
            stack.Children.Add(explanationBox);

            card.Child = stack;

            // Set up type change handler
            typeCombo.SelectionChanged += (s, e) => UpdateCreatorQuestionOptions(cq);

            // Initial setup
            UpdateCreatorQuestionOptions(cq);

            return card;
        }

        private void UpdateCreatorQuestionOptions(CreatorQuestion cq)
        {
            if (cq.TypeCombo == null || cq.OptionsPanel == null || cq.AnswerPanel == null) return;

            var selectedItem = cq.TypeCombo.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;
            var type = (QuestionType)selectedItem.Tag;

            cq.OptionsPanel.Children.Clear();
            cq.AnswerPanel.Children.Clear();
            cq.OptionTextBoxes.Clear();

            switch (type)
            {
                case QuestionType.SingleChoice:
                case QuestionType.MultipleChoice:
                    cq.OptionsPanel.Children.Add(new TextBlock
                    {
                        Text = "Options (one per line)",
                        FontSize = 13,
                        Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                        Margin = new Thickness(0, 14, 0, 6)
                    });

                    // Start with 4 option fields
                    var optStack = new StackPanel();
                    cq.OptionContainer = optStack;
                    cq.OptionsPanel.Children.Add(optStack);

                    for (int i = 0; i < 4; i++)
                    {
                        AddOptionField(cq, i);
                    }

                    var addOptBtn = new Button
                    {
                        Content = "＋ Add Option",
                        Style = (Style)FindResource("GhostButton"),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 6, 0, 0),
                        Tag = cq
                    };
                    addOptBtn.Click += (s, e) =>
                    {
                        AddOptionField(cq, cq.OptionTextBoxes.Count);
                    };
                    cq.OptionsPanel.Children.Add(addOptBtn);

                    // Correct answer
                    if (type == QuestionType.SingleChoice)
                    {
                        cq.AnswerPanel.Children.Add(new TextBlock
                        {
                            Text = "Correct Option Number (1-based)",
                            FontSize = 13,
                            Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                            Margin = new Thickness(0, 14, 0, 6)
                        });
                        var correctBox = new TextBox { Style = (Style)FindResource("ModernTextBox"), Width = 100, HorizontalAlignment = HorizontalAlignment.Left };
                        cq.CorrectAnswerBox = correctBox;
                        cq.AnswerPanel.Children.Add(correctBox);
                    }
                    else
                    {
                        cq.AnswerPanel.Children.Add(new TextBlock
                        {
                            Text = "Correct Option Numbers (comma-separated, 1-based, e.g. 1,3,4)",
                            FontSize = 13,
                            Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                            Margin = new Thickness(0, 14, 0, 6)
                        });
                        var correctBox = new TextBox { Style = (Style)FindResource("ModernTextBox"), Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
                        cq.CorrectAnswerBox = correctBox;
                        cq.AnswerPanel.Children.Add(correctBox);
                    }
                    break;

                case QuestionType.TrueFalse:
                    cq.AnswerPanel.Children.Add(new TextBlock
                    {
                        Text = "Correct Answer",
                        FontSize = 13,
                        Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                        Margin = new Thickness(0, 14, 0, 6)
                    });
                    var tfCombo = new ComboBox { Style = (Style)FindResource("ModernComboBox"), Width = 150, HorizontalAlignment = HorizontalAlignment.Left };
                    tfCombo.Items.Add(new ComboBoxItem { Content = "True" });
                    tfCombo.Items.Add(new ComboBoxItem { Content = "False" });
                    tfCombo.SelectedIndex = 0;
                    cq.TrueFalseCombo = tfCombo;
                    cq.AnswerPanel.Children.Add(tfCombo);
                    break;

                case QuestionType.OpenEnded:
                    cq.AnswerPanel.Children.Add(new TextBlock
                    {
                        Text = "Expected Answer",
                        FontSize = 13,
                        Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                        Margin = new Thickness(0, 14, 0, 6)
                    });
                    var openBox = new TextBox { Style = (Style)FindResource("ModernTextBox") };
                    cq.CorrectAnswerBox = openBox;
                    cq.AnswerPanel.Children.Add(openBox);
                    break;
            }
        }

        private void AddOptionField(CreatorQuestion cq, int index)
        {
            if (cq.OptionContainer == null) return;

            var tb = new TextBox
            {
                Style = (Style)FindResource("ModernTextBox"),
                Margin = new Thickness(0, 0, 0, 6),
                Tag = index
            };

            // Add a placeholder-like label
            var grid = new Grid();
            grid.Children.Add(tb);

            var placeholder = new TextBlock
            {
                Text = $"Option {index + 1}",
                FontSize = 14,
                Foreground = (SolidColorBrush)FindResource("TextMutedBrush"),
                IsHitTestVisible = false,
                Margin = new Thickness(16, 10, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            grid.Children.Add(placeholder);

            tb.TextChanged += (s, e) =>
            {
                placeholder.Visibility = string.IsNullOrEmpty(tb.Text) ? Visibility.Visible : Visibility.Collapsed;
            };

            cq.OptionTextBoxes.Add(tb);
            cq.OptionContainer.Children.Add(grid);
        }

        private void RemoveQuestion_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var cq = (CreatorQuestion)btn.Tag;

            _creatorQuestions.Remove(cq);
            CreatorQuestionList.Children.Clear();

            // Re-index and re-render
            for (int i = 0; i < _creatorQuestions.Count; i++)
            {
                _creatorQuestions[i].Index = i;
                var panel = CreateCreatorQuestionPanel(_creatorQuestions[i]);
                CreatorQuestionList.Children.Add(panel);
            }

            UpdateQuestionCountLabel();
        }

        private void SaveQuiz_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(CreatorTitle.Text))
            {
                ShowToast("Please enter a quiz title!");
                return;
            }

            if (_creatorQuestions.Count == 0)
            {
                ShowToast("Please add at least one question!");
                return;
            }

            var quiz = new Quiz
            {
                Title = CreatorTitle.Text.Trim(),
                Description = CreatorDescription.Text.Trim(),
                Category = string.IsNullOrWhiteSpace(CreatorCategory.Text) ? "Custom" : CreatorCategory.Text.Trim(),
                IconEmoji = string.IsNullOrWhiteSpace(CreatorEmoji.Text) ? "📝" : CreatorEmoji.Text.Trim(),
                IsBuiltIn = false
            };

            foreach (var cq in _creatorQuestions)
            {
                var question = BuildQuestionFromCreator(cq);
                if (question == null)
                {
                    ShowToast($"Please complete question {cq.Index + 1}!");
                    return;
                }
                quiz.Questions.Add(question);
            }

            _quizzes.Add(quiz);
            _storageService.SaveQuizzes(_quizzes);
            RenderQuizCards();
            ShowView("Home");
            ShowToast("Quiz saved successfully! 🎉");
        }

        private Question? BuildQuestionFromCreator(CreatorQuestion cq)
        {
            if (cq.QuestionTextBox == null || string.IsNullOrWhiteSpace(cq.QuestionTextBox.Text))
                return null;

            var selectedItem = cq.TypeCombo?.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return null;
            var type = (QuestionType)selectedItem.Tag;

            var question = new Question
            {
                Text = cq.QuestionTextBox.Text.Trim(),
                Type = type,
                Explanation = cq.ExplanationTextBox?.Text?.Trim() ?? ""
            };

            switch (type)
            {
                case QuestionType.SingleChoice:
                case QuestionType.MultipleChoice:
                    var options = cq.OptionTextBoxes
                        .Where(tb => !string.IsNullOrWhiteSpace(tb.Text))
                        .Select(tb => tb.Text.Trim())
                        .ToList();

                    if (options.Count < 2) return null;
                    question.Options = options;

                    if (cq.CorrectAnswerBox == null || string.IsNullOrWhiteSpace(cq.CorrectAnswerBox.Text))
                        return null;

                    if (type == QuestionType.SingleChoice)
                    {
                        if (!int.TryParse(cq.CorrectAnswerBox.Text.Trim(), out int num) || num < 1 || num > options.Count)
                            return null;
                        question.CorrectAnswer = (num - 1).ToString();
                    }
                    else
                    {
                        var parts = cq.CorrectAnswerBox.Text.Trim().Split(',');
                        var indices = new List<int>();
                        foreach (var p in parts)
                        {
                            if (!int.TryParse(p.Trim(), out int n) || n < 1 || n > options.Count)
                                return null;
                            indices.Add(n - 1);
                        }
                        question.CorrectAnswer = string.Join(",", indices);
                    }
                    break;

                case QuestionType.TrueFalse:
                    question.Options = new List<string> { "True", "False" };
                    question.CorrectAnswer = cq.TrueFalseCombo?.SelectedIndex.ToString() ?? "0";
                    break;

                case QuestionType.OpenEnded:
                    if (cq.CorrectAnswerBox == null || string.IsNullOrWhiteSpace(cq.CorrectAnswerBox.Text))
                        return null;
                    question.CorrectAnswer = cq.CorrectAnswerBox.Text.Trim();
                    break;
            }

            return question;
        }

        // ====================================================================
        // NAVIGATION
        // ====================================================================

        private void BackToHome_Click(object sender, RoutedEventArgs e)
        {
            _currentQuiz = null;
            RenderQuizCards();
            ShowView("Home");
        }

        private void ShowView(string viewName)
        {
            HomeView.Visibility = viewName == "Home" ? Visibility.Visible : Visibility.Collapsed;
            QuizTakingView.Visibility = viewName == "QuizTaking" ? Visibility.Visible : Visibility.Collapsed;
            ResultsView.Visibility = viewName == "Results" ? Visibility.Visible : Visibility.Collapsed;
            CreatorView.Visibility = viewName == "Creator" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // prevent card click

            var btn = (Button)sender;
            var quizId = btn.Tag?.ToString();
            if (quizId == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this quiz?", "Delete Quiz",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _quizzes.RemoveAll(q => q.Id == quizId);
                _storageService.SaveQuizzes(_quizzes);
                RenderQuizCards();
            }
        }

        // ====================================================================
        // UTILITIES
        // ====================================================================

        private void ShowToast(string message)
        {
            MessageBox.Show(message, "QuizMaster Pro", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // ====================================================================
    // HELPER CLASS FOR CREATOR STATE
    // ====================================================================

    public class CreatorQuestion
    {
        public int Index { get; set; }
        public ComboBox? TypeCombo { get; set; }
        public TextBox? QuestionTextBox { get; set; }
        public StackPanel? OptionsPanel { get; set; }
        public StackPanel? AnswerPanel { get; set; }
        public StackPanel? OptionContainer { get; set; }
        public TextBox? ExplanationTextBox { get; set; }
        public TextBox? CorrectAnswerBox { get; set; }
        public ComboBox? TrueFalseCombo { get; set; }
        public List<TextBox> OptionTextBoxes { get; set; } = new List<TextBox>();
    }
}
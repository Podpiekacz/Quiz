# QuizMaster Pro — Design Decisions

This document records the architectural and design decisions made during development.

---

## 1. Technology Choice: WPF (.NET 9)

**Decision:** Build the application using WPF (Windows Presentation Foundation) on .NET 9.

**Rationale:**
- WPF is a mature, well-supported desktop UI framework for Windows.
- It provides rich XAML-based UI capabilities with data binding, styles, templates, and animations.
- .NET 9 is the latest stable release, offering performance improvements and modern C# features.
- WPF supports complex custom styling which allows creating a modern, premium-looking dark-themed UI.

**Alternatives considered:**
- WinForms — too dated for a modern-looking UI; limited styling capabilities.
- MAUI — cross-platform but still maturing; introduces unnecessary complexity for a Windows-only app.
- Avalonia — good cross-platform option but adds third-party dependency and less tooling support.

---

## 2. Data Persistence: JSON Files in AppData

**Decision:** Store quizzes as JSON in `%APPDATA%/QuizApp/quizzes.json`.

**Rationale:**
- Simple and portable — no database setup required.
- JSON is human-readable and easy to debug.
- `AppData` folder is the standard Windows location for per-user application data.
- Newtonsoft.Json provides robust serialization/deserialization.

**Alternatives considered:**
- SQLite — overkill for simple quiz storage; adds external dependency.
- XML files — more verbose and harder to work with than JSON.
- Binary serialization — not human-readable, harder to debug.

---

## 3. Architecture: Single-Window with View Switching

**Decision:** Use a single `MainWindow` with four overlapping views (Home, Quiz Taking, Results, Creator) controlled via `Visibility` toggling.

**Rationale:**
- Simplifies navigation logic — no need for a complex routing framework.
- Reduces memory overhead compared to opening/closing multiple windows.
- Enables smooth transitions between views.
- All state can be managed in one place without inter-window communication.

**Alternatives considered:**
- Multiple windows — increases complexity, breaks the single-app feel.
- Navigation frames — adds framework complexity that isn't needed for four views.
- Full MVVM with dependency injection — overkill for the scope of this application; code-behind is appropriate.

---

## 4. Question Types: Unified Model

**Decision:** Use a single `Question` class with a `QuestionType` enum rather than separate classes per question type.

**Rationale:**
- Simplifies serialization — a single list of `Question` objects serializes cleanly.
- Reduces class proliferation — one model handles all four types.
- The `CorrectAnswer` field uses a flexible string format that adapts to each type:
  - SingleChoice/TrueFalse: zero-based index as string (e.g., "2")
  - MultipleChoice: comma-separated zero-based indices (e.g., "0,2,3")
  - OpenEnded: the expected text answer

**Alternatives considered:**
- Inheritance hierarchy (`SingleChoiceQuestion`, `OpenEndedQuestion`, etc.) — more "OOP correct" but complicates JSON serialization and UI rendering.
- Dictionary-based answers — less type-safe and harder to validate.

---

## 5. UI Design: Dark Theme with Gradient Accents

**Decision:** Implement a dark color scheme with purple-accent gradients, glassmorphism-inspired cards, and subtle animations.

**Rationale:**
- Dark themes reduce eye strain and feel more modern/premium.
- The purple (#6C63FF) to pink (#FF6584) gradient palette is visually appealing and contemporary.
- Custom-styled controls (buttons, radio buttons, checkboxes, text boxes) create a cohesive look.
- Decorative gradient orbs in the background add depth without distraction.

**Color palette:**
- Background: Deep navy (#0F0F1A → #1A1A2E)
- Cards: Dark purple-grey (#1E1E35)
- Primary accent: Vivid purple (#6C63FF)
- Secondary accent: Coral pink (#FF6584)
- Success: Mint green (#43E97B)
- Text: Near-white (#F0F0F8) with muted secondary (#A0A0C0)

---

## 6. Sample Quizzes: Three Built-In Quizzes

**Decision:** Ship with three pre-built quizzes covering General Knowledge, Science & Nature, and Technology & Computing.

**Rationale:**
- Provides immediate value on first launch — users can start playing right away.
- Each quiz has 8 questions using all four question types to demonstrate the app's capabilities.
- Marked with `IsBuiltIn = true` to differentiate from user-created quizzes.
- Built-in quizzes don't show a delete button, protecting them from accidental removal.

---

## 7. Answer Validation: Case-Insensitive for Open-Ended

**Decision:** Compare open-ended answers using case-insensitive string matching.

**Rationale:**
- Reduces frustration — "Stephen Hawking" should match "stephen hawking".
- Simple and predictable behavior for users.

**Limitations acknowledged:**
- This means partial answers or paraphrases won't match (e.g., "Hawking" won't match "Stephen Hawking").
- More sophisticated NLP-based matching was considered out of scope.

---

## 8. Quiz Creator: 1-Based Option Numbering

**Decision:** In the quiz creator, users specify correct answers using 1-based numbers.

**Rationale:**
- More intuitive for non-technical users (humans count from 1, not 0).
- Internally converted to 0-based indices for consistency with the data model.

---

## 9. Feedback System: Immediate with Explanations

**Decision:** Show correct/incorrect feedback immediately after submitting each answer, with optional explanations.

**Rationale:**
- Immediate feedback is proven to be more effective for learning than delayed feedback.
- Explanations add educational value beyond simple right/wrong indicators.
- Users must explicitly click "Next Question" to proceed, giving them time to read the explanation.

**Alternatives considered:**
- Show all results only at the end — less educational, more anxiety-inducing.
- Timed auto-advance — too rushed for learning purposes.

---

## 10. Results Display: Tiered Scoring with Emoji

**Decision:** Show results with a circular progress indicator and tiered messaging (Excellent ≥80%, Good ≥60%, Not Bad ≥40%, Keep Studying <40%).

**Rationale:**
- Visual progress circle provides an instant sense of performance.
- Encouraging messages at each tier motivate continued learning.
- Emoji icons (🏆, 👏, 💪, 📚) add personality and make results memorable.
- Full question review below allows learning from mistakes.

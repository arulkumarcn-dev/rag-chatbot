namespace RAGChatbot.API.Models;

public class QuizGenerationRequest
{
    public string Topic { get; set; } = string.Empty;
    public int QuestionCount { get; set; } = 10;
}

public class QuizQuestion
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswerIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

public class Quiz
{
    public string Topic { get; set; } = string.Empty;
    public List<QuizQuestion> Questions { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

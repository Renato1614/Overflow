using QuestionService.DTO;
using System.ComponentModel.DataAnnotations;

namespace QuestionService.Models;

public class Question
{
    [MaxLength(length: 36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [MaxLength(300)]
    public required string Title { get; set; }
    [MaxLength(5000)]
    public required string Content { get; set; }
    [MaxLength(336)]
    public required string AskerId { get; set; } = Guid.NewGuid().ToString();
    [MaxLength(300)]
    public required string AskerDisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int ViewCount { get; set; }
    public List<string> TagSlugs { get; set; } = [];
    public bool HasAccetedAnswer { get; set; }
    public int Votes { get; set; }


    public void Update(CreateQuestionDto dto)
    {
        Title = dto.Title;
        Content = dto.Content;
        TagSlugs = dto.Tags;
        UpdatedAt = DateTime.UtcNow;
    }
}

using QuestionService.Validator;
using System.ComponentModel.DataAnnotations;

namespace QuestionService.DTO
{
    public record CreateQuestionDto(
        [Required] string Title, 
        [Required] string Content, 
        [Required][TagListValidator(1,5)] List<string> Tags);
}

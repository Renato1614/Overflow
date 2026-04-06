using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTO;
using QuestionService.Models;
using QuestionService.Services;
using System.Security.Claims;
using Wolverine;
using Wolverine.Shims.MediatR;

namespace QuestionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController(QuestionDbContext db, IMessageBus bus, TagService tagService) : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Question>> CreateQuestion(CreateQuestionDto request)
        {

            if (!await tagService.AreTagsValidAsync(request.Tags))
                return BadRequest("Invalid tags");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = User.FindFirstValue("name");

            if (userId is null || name is null) return BadRequest("Cannot get user details from token");

            var question = new Question()
            {
                Title = request.Title,
                Content = request.Content,
                TagSlugs = request.Tags,
                AskerId = userId,
                AskerDisplayName = name
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync();

            await PublishCreatedQuestion(question);

            return Created($"/questions/{question.Id}", question);

        }

        private async Task PublishCreatedQuestion(Question question)
        {
            await bus.PublishAsync(new QuestionCreated(
                            question.Id,
                            question.Title,
                            question.Content,
                            question.CreatedAt,
                            question.TagSlugs
                        ));
        }

        [HttpGet]
        public async Task<ActionResult<List<Question>>> GetQuestionsByTag(string? tag)
        {
            var query = db.Questions.AsQueryable();
            if (!string.IsNullOrEmpty(tag))
                query = query.Where(x => x.TagSlugs.Contains(tag));

            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestionById(string id)
        {
            var question = await db.Questions.FindAsync(id);
            if (question is null) return NotFound();

            await db.Questions.Where(x => x.Id == id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1));

            return Ok(question);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateQuestion(string id, CreateQuestionDto dto)
        {
            Question? question = await db.Questions.FindAsync(id);
            if (question is null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != question.AskerId) return Forbid();

            if (!await tagService.AreTagsValidAsync(dto.Tags))
                return BadRequest("Invalid tags");

            question.Update(dto);

            await bus.PublishAsync(new QuestionUpdated(question.Id, question.Title, question.Content, question.TagSlugs.ToArray()));

            await db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteQuestion(string id)
        {
            Question? question = await db.Questions.FindAsync(id);
            if (question is null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != question.AskerId) return Forbid();

            db.Questions.Remove(question);
            await db.SaveChangesAsync();

            await bus.PublishAsync(new QuestionDeleted(id));
    
            return NoContent();
        }
    }
}

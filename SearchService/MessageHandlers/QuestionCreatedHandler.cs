using Contracts;
using JasperFx.CodeGeneration.Frames;
using SearchService.Models;
using SearchService.Utils;
using System.Text.RegularExpressions;
using Typesense;

namespace SearchService.MessageHandlers;

public class QuestionCreatedHandler(ITypesenseClient client)
{
    public async Task HandleAsync(QuestionCreated message)
    {
        var created = new DateTimeOffset(message.CreatedAt).ToUnixTimeSeconds();
        var doc = new SearchQuestion() {
            Id = message.QuestionId,
            Title = message.Title,
            Content = StripHelper.StripHtml(message.Content),
            CreatedAt = created,
            Tags = message.Tags.ToArray()
        };

        await client.CreateDocument("questions", doc);

        Console.WriteLine($"Created question with id {message.QuestionId}");

    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SearchService.Models;
using System.Text.RegularExpressions;
using Typesense;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<IResult> Search([FromServices]ITypesenseClient client,string query)
        {
            string? tag = null;
            var tagMatch = Regex.Match(query, @"\[(.*?)\]");
            if (tagMatch.Success)
            {
                tag = tagMatch.Groups[1].Value;
                query = query.Replace(tagMatch.Value, "").Trim();
            }

            var searchParams = new SearchParameters(query, "title,content");

            if (!string.IsNullOrEmpty(tag))
            {
                searchParams.FacetBy = $"tag:=[{tag}]";
            }

            try
            {
                var result = await client.Search<SearchQuestion>("questions", searchParams);
                return Results.Ok(result.Hits.Select(hit => hit.Document));
            }
            catch (Exception e)
            {
                return Results.Problem("Typesense search failed", e.Message);
            }
        }
    }
}

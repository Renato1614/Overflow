using System.Text.RegularExpressions;

namespace SearchService.Utils
{
    public static class StripHelper
    {
        public static string StripHtml(string content)
                => Regex.Replace(content, "<.*?>", string.Empty);
    }
}

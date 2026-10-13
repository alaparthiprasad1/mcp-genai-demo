using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpDemo.Tools;

/// <summary>
/// Three small, self-contained tools that show what an MCP server does:
/// an AI assistant calls a named tool with inputs, and the server runs
/// real code and returns a real result. No external services, no keys,
/// no network calls — everything runs locally in this process.
/// </summary>
[McpServerToolType]
public static class DemoTools
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "and", "or", "but", "if", "then", "else", "when",
        "at", "by", "for", "with", "about", "against", "between", "into",
        "through", "during", "before", "after", "above", "below", "to",
        "from", "up", "down", "in", "out", "on", "off", "over", "under",
        "again", "further", "once", "here", "there", "all", "any", "both",
        "each", "few", "more", "most", "other", "some", "such", "no", "nor",
        "not", "only", "own", "same", "so", "than", "too", "very", "can",
        "will", "just", "should", "now", "is", "am", "are", "was", "were",
        "be", "been", "being", "have", "has", "had", "having", "do", "does",
        "did", "doing", "i", "you", "he", "she", "it", "we", "they", "me",
        "him", "her", "us", "them", "my", "your", "his", "its", "our",
        "their", "this", "that", "these", "those", "of", "as", "at"
    };

    [McpServerTool]
    [Description("Analyze a piece of text: word and sentence counts, average word length, estimated reading time, and the five most frequent meaningful words.")]
    public static string TextInsights(
        [Description("The text to analyze.")] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "No text was provided, so there is nothing to analyze.";
        }

        var words = text
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim(".,;:!?\"'()[]{}<>/\\|@#$%^&*-_=+".ToCharArray()))
            .Where(w => w.Length > 0)
            .ToList();

        var sentences = text
            .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Count(s => !string.IsNullOrWhiteSpace(s));

        var averageWordLength = words.Count == 0
            ? 0
            : Math.Round(words.Average(w => w.Length), 1);

        var readingMinutes = Math.Max(1, (int)Math.Ceiling(words.Count / 200.0));

        var topWords = words
            .Where(w => !StopWords.Contains(w) && w.Length > 1)
            .GroupBy(w => w.ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(5)
            .Select(g => $"{g.Key} ({g.Count()})");

        var result = new
        {
            wordCount = words.Count,
            sentenceCount = sentences,
            averageWordLength,
            estimatedReadingTimeMinutes = readingMinutes,
            topWords = topWords.ToList(),
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Turn a title into a clean URL slug (lowercase, words joined by hyphens) and report whether the result is URL-safe.")]
    public static string SlugifyTitle(
        [Description("The title to convert, e.g. 'My First MCP Demo!'.")] string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "No title was provided, so no slug could be created.";
        }

        var slug = new StringBuilder();
        var lastWasHyphen = true; // avoids a leading hyphen

        foreach (var ch in title.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                slug.Append(ch);
                lastWasHyphen = false;
            }
            else if (!lastWasHyphen)
            {
                slug.Append('-');
                lastWasHyphen = true;
            }
        }

        var slugText = slug.ToString().Trim('-');
        var isUrlSafe = slugText.Length > 0 &&
            slugText.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');

        var result = new
        {
            originalTitle = title,
            slug = slugText,
            characterCount = slugText.Length,
            isUrlSafe,
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Break a goal into a short, ordered plan of practical steps with rough time estimates, so an assistant can walk someone through the work.")]
    public static string PlanTask(
        [Description("The goal to plan, e.g. 'Ship a small demo API'.")] string goal,
        [Description("How many steps the plan should have, between 2 and 6. Defaults to 4.")] int steps = 4)
    {
        if (string.IsNullOrWhiteSpace(goal))
        {
            return "No goal was provided, so no plan could be created.";
        }

        var stepCount = Math.Clamp(steps, 2, 6);
        var phases = new (string Name, string Detail, int Minutes)[]
        {
            ("Clarify the outcome", $"Write down what 'done' means for: {goal}. List the must-haves and the nice-to-haves.", 15),
            ("Sketch the approach", "Pick the smallest design that covers the must-haves. Note the main building blocks and any unknowns.", 30),
            ("Build the core", "Implement the essential path first. Keep it small enough to test in one sitting.", 60),
            ("Test it end to end", "Run the real thing once from start to finish and fix what breaks before polishing.", 30),
            ("Polish and document", "Clean up rough edges and write the short guide someone else needs to run it.", 30),
            ("Share it", "Put it somewhere others can try it, and note what you would improve next.", 15),
        };

        var plan = phases
            .Take(stepCount)
            .Select((p, i) => new
            {
                step = i + 1,
                name = p.Name,
                detail = p.Detail,
                estimatedMinutes = p.Minutes,
            })
            .ToList();

        var result = new
        {
            goal = goal.Trim(),
            totalEstimatedMinutes = plan.Sum(p => p.estimatedMinutes),
            steps = plan,
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
}

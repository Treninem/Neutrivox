namespace Neutrivox.Services;

public sealed record OwenReplicationProgress(int? Percent, string Message, bool IsError);

/// <summary>Parses the documented redirected stdout/stderr format conservatively.</summary>
public static class OwenReplicationOutputParser
{
    public static IReadOnlyList<OwenReplicationProgress> Parse(string stdout, string stderr)
    {
        var result = new List<OwenReplicationProgress>();
        AddLines(result, stdout, false);
        AddLines(result, stderr, true);
        return result;
    }

    private static void AddLines(List<OwenReplicationProgress> result, string text, bool error)
    {
        foreach (var raw in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            int? percent = null;
            var match = System.Text.RegularExpressions.Regex.Match(raw, @"(?<!\d)(\d{1,3})%?(?!\d)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var value) && value is >= 0 and <= 100)
                percent = value;
            result.Add(new(percent, raw, error));
        }
    }
}

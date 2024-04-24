using Svg2Gcode.GCode;

namespace GCode.Trivias
{
    public static class TriviaExtensions
    {
        public static string ToText(this IEnumerable<TriviaSyntax>? trivia)
        {
            if (trivia is null) return "";
            return string.Join("", trivia.Select(t => t.Text));
        }
        public static T[] WithTrailingTrivia<T>(this T[] tokens, TriviaSyntax[] trivia) where T : Token
        {
            tokens = tokens.ToArray();
            tokens[^1] = (T)tokens[^1].WithTrailingTrivia(trivia);
            return tokens;
        }
    }
}
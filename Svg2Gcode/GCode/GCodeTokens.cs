using Svg2Gcode.Tools;

namespace Svg2Gcode.GCode
{
    public record Trivia(string Text);
    public record EndOfLineTrivia() : Trivia("\n");
    public record WhitespaceTrivia(int Length) : Trivia(new string(' ', Length));
    public record CommentTrivia(string Comment) : Trivia($"({Comment})");
    public record EndOfLineCommentTrivia(string Comment) : Trivia($";{Comment}");

    public static class TriviaExtensions
    {
        public static string ToText(this IEnumerable<Trivia>? trivia)
        {
            if (trivia is null) return "";
            return String.Join("", trivia.Select(t => t.Text));
        }
        //public static T[] WithLeadingTrivia<T>(this T[] tokens, Trivia[] trivia) where T : Token
        //{
        //    tokens = tokens.ToArray();
        //    tokens[0] = tokens[0].WithTrailingTrivia(trivia);
        //    return tokens;
        //}
        //public static T[] WithTrailingTrivia<T>(this T[] tokens, Trivia[] trivia) where T : Token
        //{
        //    tokens = tokens.ToArray();
        //    tokens[^1] = tokens[^1].WithTrailingTrivia(trivia);
        //    return tokens;
        //}
    }

    public abstract record Token(Trivia[]? LeadingTrivia = null, Trivia[]? TrailingTrivia = null)
    {
        public abstract string Text { get; }
        public string FullText => $"{LeadingTrivia.ToText()}{Text}{TrailingTrivia.ToText()}";

        public abstract Token WithLeadingTrivia(params Trivia[] trivia);
        public abstract Token WithTrailingTrivia(params Trivia[] trivia);
    }
    public record CommandToken(char Key, int Number, Trivia[]? LeadingTrivia = null, Trivia[]? TrailingTrivia = null) : Token(LeadingTrivia, TrailingTrivia)
    {
        public override string Text => $"{Key}{Number:00}";

        public override CommandToken WithLeadingTrivia(params Trivia[] trivia) => new(Key, Number, trivia, TrailingTrivia);
        public override CommandToken WithTrailingTrivia(params Trivia[] trivia) => new(Key, Number, LeadingTrivia, trivia);
    }
    public record ArgumentToken(char Key, double Value, Trivia[]? LeadingTrivia = null, Trivia[]? TrailingTrivia = null) : Token(LeadingTrivia, TrailingTrivia)
    {
        public override string Text => $"{Key}{Value:0.000}";

        public override ArgumentToken WithLeadingTrivia(params Trivia[] trivia) => new(Key, Value, trivia, TrailingTrivia);
        public override ArgumentToken WithTrailingTrivia(params Trivia[] trivia) => new(Key, Value, LeadingTrivia, trivia);
    }
    public abstract record Statement(Token[] Tokens)
    {
        public string Text => String.Join(" ", Tokens.Select(token => token.Text));

        public abstract Statement WithLeadingTrivia(Trivia[] trivia);
        public abstract Statement WithTrailingTrivia(Trivia[] trivia);
    }

    public record CommandStatement(CommandToken Command, ArgumentToken[]? Arguments = null) : Statement(Enumerable.Empty<Token>()
        .Concat(Command)
        .Concat(Arguments ?? Enumerable.Empty<ArgumentToken>())
        .WhereNotNull()
        .ToArray())
    {

        public override CommandStatement WithLeadingTrivia(Trivia[] trivia) => new CommandStatement(Command.WithLeadingTrivia(trivia), Arguments);
        public override CommandStatement WithTrailingTrivia(Trivia[] trivia) => new CommandStatement(Command, Arguments.WithTrailingTrivia(trivia));
    }
}
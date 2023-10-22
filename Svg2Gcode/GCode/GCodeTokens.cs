using Svg2Gcode.Tools;

namespace Svg2Gcode.GCode
{
    public abstract record Token
    {
        public abstract string Text { get; }
    }
    public record CommandToken(int Number) : Token
    {
        public override string Text => $"G{Number:00}";
    }
    public record ArgumentToken(string Name, double Value) : Token
    {
        public override string Text => $"{Name}{Value:N3}";
    }
    public record CommentToken(string Comment) : Token
    {
        public override string Text => $"({Comment})";
    }
    public abstract record Statement(Token[] Tokens)
    {
        public string Text => string.Join(" ", Tokens.Select(token => token.Text));
    }

    public record CommentStatement(CommentToken Comment) : Statement(new[] { Comment });


    public record CommandStatement(CommandToken Command, ArgumentToken[] Arguments, CommentToken? Comment = null) : Statement(Enumerable.Empty<Token>()
        .Concat(Command)
        .Concat(Arguments)
        .Concat(Comment)
        .WhereNotNull()
        .ToArray());
}
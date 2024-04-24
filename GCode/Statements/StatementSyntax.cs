using Dalby.GCode.Trivias;
using GCode.Trivias;
using Svg2Gcode.GCode;

namespace GCode.Statements
{
    public abstract record StatementSyntax(Token[] Tokens)
    {
        public string FullText => string.Join("", Tokens.Select(token => token.FullText));

        public abstract StatementSyntax WithLeadingTrivia(params TriviaSyntax[] trivia);
        public abstract StatementSyntax WithTrailingTrivia(params TriviaSyntax[] trivia);

        public override string ToString() => string.Join(" ", Tokens.Select(token => token.Text));
    }

    public static class StatementExtensions
    {
        public static void ToSyntaxTree(this IEnumerable<StatementSyntax> statements, string filePath)
        {
            using Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            statements.ToSyntaxTree(stream);
        }
        public static void ToSyntaxTree(this IEnumerable<StatementSyntax> statements, Stream stream)
        {
            string formatToken(Token token) => token switch
            {
                CommandToken => "CT",
                ArgumentToken => "AT"
            };
            string formatTrivia(TriviaSyntax trivia) => trivia switch
            {
                WhitespaceTrivia => "WS",
                EndOfLineCommentTrivia => "EOLCT",
                EndOfLineTrivia => "EOL"
            };

            StreamWriter writer = new(stream);
            foreach (StatementSyntax statement in statements)
            {
                writer.WriteLine(statement.FullText.Replace("\n", "[EOL]"));
                foreach (Token token in statement.Tokens)
                {
                    writer.WriteLine($"  - {formatToken(token)} [{token.Text}]");
                    if (token.LeadingTrivia is not null) writer.WriteLine($"    - L: {String.Join(" | ", token.LeadingTrivia.Select(formatTrivia))}");
                    if (token.TrailingTrivia is not null) writer.WriteLine($"    - T: {String.Join(" | ", token.TrailingTrivia.Select(formatTrivia))}");
                }
            }
            writer.Flush();
        }
    }
}
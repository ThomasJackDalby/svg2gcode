using GCode.Trivias;
using Svg2Gcode.GCode;
using System.Linq;
using Utils.Extensions;

namespace GCode.Statements
{
    public record CommandStatementSyntax(CommandToken Command, ArgumentToken[]? Arguments = null) : StatementSyntax(Enumerable.Empty<Token>()
        .Concat(Command)
        .Concat(Arguments ?? Enumerable.Empty<ArgumentToken>())
        .WhereNotNull()
        .ToArray())
    {
        public override CommandStatementSyntax WithLeadingTrivia(params TriviaSyntax[] trivia) => new(Command.WithLeadingTrivia(trivia), Arguments);
        public override CommandStatementSyntax WithTrailingTrivia(params TriviaSyntax[] trivia)
        {
            if (Arguments is not null && Arguments.Any()) return new(Command, Arguments.WithTrailingTrivia(trivia));
            return new(Command.WithTrailingTrivia(trivia));
        }
    }
}
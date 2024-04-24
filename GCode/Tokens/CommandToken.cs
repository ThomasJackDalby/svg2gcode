using GCode.Trivias;

namespace Svg2Gcode.GCode
{
    public record CommandToken(char Key, int Number, TriviaSyntax[]? LeadingTrivia = null, TriviaSyntax[]? TrailingTrivia = null) : Token(LeadingTrivia, TrailingTrivia)
    {
        public override string Text => $"{Key}{Number:00}";

        public override CommandToken WithLeadingTrivia(TriviaSyntax[]? trivia) => new(Key, Number, trivia, TrailingTrivia);
        public override CommandToken WithTrailingTrivia(TriviaSyntax[]? trivia) => new(Key, Number, LeadingTrivia, trivia);
    }
}
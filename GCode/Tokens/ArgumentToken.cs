using GCode.Trivias;

namespace Svg2Gcode.GCode
{
    public record ArgumentToken(char Key, double Value, TriviaSyntax[]? LeadingTrivia = null, TriviaSyntax[]? TrailingTrivia = null) : Token(LeadingTrivia, TrailingTrivia)
    {
        public override string Text => $"{Key}{Value:0.000}";

        public override ArgumentToken WithLeadingTrivia(params TriviaSyntax[] trivia) => new(Key, Value, trivia, TrailingTrivia);
        public override ArgumentToken WithTrailingTrivia(params TriviaSyntax[] trivia) => new(Key, Value, LeadingTrivia, trivia);
    }
}
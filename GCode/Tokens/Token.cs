using GCode;
using GCode.Trivias;

namespace Svg2Gcode.GCode
{
    public abstract record Token(TriviaSyntax[]? LeadingTrivia = null, TriviaSyntax[]? TrailingTrivia = null)
    {
        public abstract string Text { get; }
        public string FullText => $"{LeadingTrivia.ToText()}{Text}{TrailingTrivia.ToText()}";

        public abstract Token WithLeadingTrivia(TriviaSyntax[]? trivia);
        public abstract Token WithTrailingTrivia(TriviaSyntax[]? trivia);
    }
}
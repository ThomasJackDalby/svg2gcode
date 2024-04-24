namespace GCode.Trivias
{
    public record EndOfLineCommentTrivia(string Comment) : TriviaSyntax($"; {Comment}");
}
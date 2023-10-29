using Svg2Gcode.Tools;

namespace Svg2Gcode.Svg.Paths
{
    public record MoveToCommand(double X, double Y, bool IsRelative = false) : PathCommand(IsRelative)
    {
        public override IEnumerable<(bool, Path2D)> GetPaths(Vector2D commandStart, Vector2D start)
        {
            Vector2D end = IsRelative ? new(start.X + X, start.Y + Y) : new(X, Y);
            yield return (false, new Path2D(start, end));
        }
    }
}
using Dalby.Maths.Geometry;

namespace Svg2Gcode.Svg.Paths
{
    public record VerticalLineToCommand(double Y, bool IsRelative = false) : PathCommand(IsRelative)
    {
        public override IEnumerable<(bool, Path2D)> GetPaths(Vector2D commandStart, Vector2D start)
        {
            double y = IsRelative ? start.Y + Y : Y;
            yield return (true, new Path2D(start, new(start.X, y)));
        }
    }
}   
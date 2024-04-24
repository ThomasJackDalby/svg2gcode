using Svg2Gcode.Spatial;
using Utils.Spatial;

namespace Svg2Gcode.Svg.Paths
{
    public record HorizontalLineToCommand(double X, bool IsRelative = false) : PathCommand(IsRelative)
    {
        public override IEnumerable<(bool, Path2D)> GetPaths(Vector2D commandStart, Vector2D start)
        {
            double x = IsRelative ? start.X + X : X;
            yield return (true, new Path2D(start, new(x, start.Y)));
        }
    }
}
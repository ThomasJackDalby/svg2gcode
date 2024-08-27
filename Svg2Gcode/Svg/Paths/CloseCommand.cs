using Dalby.Maths.Geometry;

namespace Svg2Gcode.Svg.Paths
{
    public record CloseCommand() : PathCommand
    {
        public override IEnumerable<(bool, Path2D)> GetPaths(Vector2D pathStart, Vector2D commandStart)
        {
            bool active = pathStart != commandStart;
            yield return (active, new Path2D(commandStart, pathStart));
        }
    }
}
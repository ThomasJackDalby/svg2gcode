using Svg2Gcode.Tools;

namespace Svg2Gcode.Svg.Paths
{
    public record CloseCommand() : PathCommand
    {
        public override IEnumerable<(bool, Path2D)> GetPaths(Vector2D pathStart, Vector2D commandStart)
        {
            yield return (true, new Path2D(commandStart, pathStart));
        }
    }
}
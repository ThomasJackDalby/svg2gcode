using Svg2Gcode.Tools;

namespace Svg2Gcode.Svg
{
    public abstract class Shape : Element
    {
        public abstract IEnumerable<Segment2D>? Intersect(Segment2D segment);
        public abstract IEnumerable<Path2D> GetPaths();
    }
}
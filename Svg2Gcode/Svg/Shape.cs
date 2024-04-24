using Svg2Gcode.Spatial;
using Utils.Spatial;

namespace Svg2Gcode.Svg
{
    public abstract class Shape : Element
    {
        public abstract IEnumerable<Path2D>? Intersect(Segment2D segment);
        public abstract IEnumerable<Path2D> GetPaths();
    }
}
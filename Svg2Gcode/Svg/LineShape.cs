using Svg2Gcode.Spatial;
using System.Xml.Linq;
using Utils.Spatial;

namespace Svg2Gcode.Svg
{
    public class LineShape : Shape
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }
        
        public override IEnumerable<Path2D> GetPaths()
        {
            yield return new Path2D(new Vector2D(X1, Y1), new Vector2D(X2, Y2));
        }
        public override IEnumerable<Path2D>? Intersect(Segment2D segment) => null; // No masking required
        public static LineShape? From(XElement xElement)
        {
            LineShape shape = new();
            shape.X1 = Double.Parse(xElement.Attribute("x1")?.Value ?? "");
            shape.X2 = Double.Parse(xElement.Attribute("x2")?.Value ?? "");
            shape.Y1 = Double.Parse(xElement.Attribute("y1")?.Value ?? "");
            shape.Y2 = Double.Parse(xElement.Attribute("y2")?.Value ?? "");
            return shape;
        }
    }
}
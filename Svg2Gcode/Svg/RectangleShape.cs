using Svg2Gcode.Spatial;
using Svg2Gcode.Svg.Paths;
using System.Xml;
using System.Xml.Linq;
using Utils.Spatial;

namespace Svg2Gcode.Svg
{
    public class RectangleShape : Shape
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }

        public override IEnumerable<Path2D> GetPaths()
        {
            yield return new Path2D(new Vector2D(X, Y), new Vector2D(X + Width, Y), new Vector2D(X + Width, Y + Height), new Vector2D(X, Y + Height), new Vector2D(X, Y));
        }
        public override IEnumerable<Path2D>? Intersect(Segment2D segment)
        {
            throw new NotImplementedException();
        }

        public static RectangleShape? From(XElement xElement)
        {
            double x = xElement.GetAttributeValueOrDefault<double>("x", 0);
            double y = xElement.GetAttributeValueOrDefault<double>("y", 0);
            double width = xElement.GetAttributeValueOrDefault<double>("width", 0);
            double height = xElement.GetAttributeValueOrDefault<double>("height", 0);
            double rx = xElement.GetAttributeValueOrDefault<double>("rx", 0);
            double ry = xElement.GetAttributeValueOrDefault<double>("ry", 0);
            return new RectangleShape
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                RadiusX = rx,
                RadiusY = ry
            };
        }
    }
}

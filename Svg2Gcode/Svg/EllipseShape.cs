using Svg2Gcode.Spatial;
using System.Xml.Linq;
using Utils.Spatial;

namespace Svg2Gcode.Svg
{
    public class EllipseShape : Shape
    {
        public double CX { get; set; }
        public double CY { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }

        public override IEnumerable<Path2D> GetPaths()
        {
            // TODO
            yield break;
        }

        public override IEnumerable<Path2D>? Intersect(Segment2D segment)
        {
            throw new NotImplementedException();
        }

        public static EllipseShape? From(XElement xElement)
        {
            double x = xElement.GetAttributeValueOrDefault<double>("x", 0);
            double y = xElement.GetAttributeValueOrDefault<double>("y", 0);
            double rx = xElement.GetAttributeValueOrDefault<double>("rx", 0);
            double ry = xElement.GetAttributeValueOrDefault<double>("ry", 0);
            return new EllipseShape
            {
                CX = x,
                CY = y,
                RadiusX = rx,
                RadiusY = ry
            };
        }
    }
}

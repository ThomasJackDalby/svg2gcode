using Svg2Gcode.Spatial;
using Svg2Gcode.Svg.Paths;
using System.Xml.Linq;
using Utils.Spatial;

namespace Svg2Gcode.Svg
{
    public class PathShape : Shape
    {
        public List<PathCommand> Commands { get; } = new();

        public override IEnumerable<Path2D> GetPaths()
        {
            Vector2D start = new(0, 0);
            Vector2D pathStart = start;
            foreach (PathCommand command in Commands)
            {
                foreach ((bool active, Path2D path) in command.GetPaths(pathStart, start))
                {
                    if (active) yield return path;
                    start = path.Points.LastOrDefault() ?? start;
                    if (command is CloseCommand) pathStart = start;
                }
            }
        }
        public override IEnumerable<Path2D>? Intersect(Segment2D segment)
        {
            // masking for paths is tricky as fill is hard to evaluate
            return null;
        }
        //public static PathShape? From(IEnumerable<Path2D> paths)
        //{
        //    PathShape shape = new();
        //    foreach(Path2D path in paths)
        //    {
        //        shape.Commands.Add(new MoveToCommand(path.Points[0].X, path.Points[0].Y, false));
        //        foreach(var point in path.Points.Skip(1)) shape.Commands.Add(new MoveToCommand(point.X, point.Y, false));

        //    }
        //}
        public static PathShape? From(XElement xElement)
        {
            XAttribute? dataAttribute = xElement.Attribute("d");
            if (dataAttribute is null) return null;

            PathShapeParser parser = new();
            return parser.Parse(dataAttribute.Value);
        }
    }
}
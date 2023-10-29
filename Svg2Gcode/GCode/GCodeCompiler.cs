using Svg2Gcode.Svg;
using Svg2Gcode.Tools;

namespace Svg2Gcode.GCode
{
    public class GCodeCompiler
    {
        private readonly double penDownPosition = 0;
        private readonly double penUpPosition = 10;
        private readonly double feedRate = 1000;

        public void Compile(string filePath, SvgDocument svgDocument)
        {
            IEnumerable<Path2D> paths = extractPaths(svgDocument);
            IEnumerable<Statement> statements = compile(paths);
            save(filePath, statements);
        }

        private IEnumerable<Path2D> extractPaths(SvgDocument svgDocument)
        {
            foreach (Shape shape in svgDocument.GetShapes()) // order these by depth? do top first?
            {
                // yield return new CommentStatement(new CommentToken(shape.GetType().Name));
                foreach (Path2D path in shape.GetPaths())
                {
                    // want to split paths which intersect current masks
                    yield return path;
                }
            }
        }
        private IEnumerable<Statement> compile(IEnumerable<Path2D> paths)
        {
            //yield return new CommentStatement(new EndOfLineCommentToken($"Name: Converted SVG Document."));
            //yield return new CommentStatement(new EndOfLineCommentToken($"Date: {DateTime.Now}"));

            Trivia[] pretrivia = new Trivia[]
            {
                new EndOfLineCommentTrivia($"Name: Converted SVG Document."),
                new EndOfLineCommentTrivia($"Date: {DateTime.Now}")
            };
            
            yield return Commands.G21_Millimetres();
            yield return Commands.G90_AbsoluteCoordinates();
            yield return Commands.G17_XYPlane();
            yield return Commands.G94_UnitsPerMinuteFeedRateMode();
            yield return Commands.M3_TurnOnSpindle(1000);

            Path2D? previous = null;
            foreach (Path2D path in paths)
            {
                Vector2D start = path.Points[0];
                bool setFeed = false;
                if (previous is null || start.X != previous.Points.Last().X || start.Y != previous.Points.Last().Y)
                {
                    yield return Commands.G0_RapidMotion(z: penUpPosition);
                    yield return Commands.G0_RapidMotion(start.X, start.Y);
                    yield return Commands.G0_RapidMotion(z: penDownPosition);
                    setFeed = true;
                }

                // draw the line
                for (int i = 1; i < path.Points.Length; i++)
                {
                    Vector2D point = path.Points[i];
                    double? f = setFeed ? feedRate : null;
                    setFeed = false;
                    yield return Commands.G1_CoordinatedMotion(x: point.X, y: point.Y, f: f);
                }
                previous = path;
            }

            yield return Commands.G0_RapidMotion(z: penUpPosition);
            yield return Commands.G0_RapidMotion(x: 0, y: 0);
        }
        private void save(string filePath, IEnumerable<Statement> statements)
        {
            using Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            using StreamWriter writer = new(stream);

            foreach (Statement statement in statements) writer.WriteLine(statement.Text);
        }
    }
}

using Microsoft.VisualBasic;
using Svg2Gcode.GCode;
using System.Text;
using System.Xml.Linq;

namespace Svg2Gcode.Svg
{
    public class PathShape : Shape
    {
        public List<PathCommand> Commands { get; } = new();

        public static PathShape? From(XElement xElement)
        {
            PathShape path = new();

            XAttribute? dataAttribute = xElement.Attribute("d");
            if (dataAttribute is null) return null;

            bool skipWhiteSpace = false;
            char command = ' ';
            List<string> arguments = new();
            StringBuilder builder = new();
            for (int i = 0; i < dataAttribute.Value.Length; i++)
            {
                char c = dataAttribute.Value[i];
                if (skipWhiteSpace && c == ' ') continue;
                skipWhiteSpace = false;
                if (char.IsLetter(c) && c != 'e') // new command
                {
                    static IEnumerable<double[]> getArguments(List<string> arguments, int amount)
                    {
                        int argumentIndex = 0;
                        while (arguments.Count > argumentIndex)
                        {
                            if (arguments.Count < argumentIndex + amount - 1) throw new Exception();
                            double[] args = new double[amount];
                            for (int i = 0; i < amount; i++) args[i] = double.Parse(arguments[argumentIndex + i]);
                            yield return args;
                            argumentIndex += amount;
                        }
                    }

                    if (i > 0) // process the old command
                    {
                        bool isRelative = char.IsAsciiLetterLower(command);
                        command = char.ToLower(command);
                        if (command == 'm' || command == 'M') path.Commands.AddRange(getArguments(arguments, 2).Select(args => new MoveToCommand(args[0], args[1], isRelative)));
                        else if (command == 'l' || command == 'L') path.Commands.AddRange(getArguments(arguments, 2).Select(args => new LineToCommand(args[0], args[1], isRelative)));
                        else if (command == 'h' || command == 'H') path.Commands.AddRange(getArguments(arguments, 1).Select(args => new HorizontalLineToCommand(args[0], isRelative)));
                        else if (command == 'v' || command == 'V') path.Commands.AddRange(getArguments(arguments, 1).Select(args => new VerticalLineToCommand(args[0], isRelative)));
                        else if (command == 'c' || command == 'C') path.Commands.AddRange(getArguments(arguments, 6).Select(args => new CurveToCommand(args[0], args[1], args[2], args[3], args[4], args[5], isRelative)));
                        else if (command == 'z' || command == 'Z') path.Commands.Add(new CloseCommand());
                        else throw new Exception();
                    }

                    // record new command
                    command = c;
                    arguments.Clear();
                    skipWhiteSpace = true;
                }
                else if (c == ' ' || c == ',')
                {
                    arguments.Add(builder.ToString());
                    builder.Clear();
                    skipWhiteSpace = true;
                }
                else builder.Append(c);
            }

            return path;
        }

        public override PenState Transform(PenState state)
        {
            foreach (PathCommand command in Commands) state = command.Transform(state);
            return state;
        }
        public override IEnumerable<Statement> GetGCode(PenState state)
        {
            foreach (PathCommand command in Commands)
            {
                foreach (Statement statement in command.GetGCode(state)) yield return statement;
                state = command.Transform(state);
            }
        }
    }
    public record PenState(double X, double Y, double Z);
    public abstract record PathCommand
    {
        public virtual PenState Transform(PenState state) => state;
        public virtual IEnumerable<Statement> GetGCode(PenState state) => Enumerable.Empty<Statement>();
    }
    public record CurveToCommand(double X1, double Y1, double X2, double Y2, double X, double Y, bool IsRelative = false) : PathCommand
    {
        public override PenState Transform(PenState state) => IsRelative
            ? new(state.X + X, state.Y + Y, 0)
            : new(X, Y, 0);
        public override IEnumerable<Statement> GetGCode(PenState state)
        {
            yield return new CommentStatement(new CommentToken(nameof(CurveToCommand)));
            if (state.Z != 0) yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("Z", 0) }, new CommentToken("Pen Down"));

            (double x1, double y1, double x2, double y2, double x, double y) = IsRelative
                ? (state.X + X1, state.Y + Y1, state.X + X2, state.Y + Y2, state.X + X, state.Y + Y)
                : (X1, Y1, X2, Y2, X, Y);

            int amount = 10;
            double dt = 1.0 / amount;
            BezierFunction curveX = new(state.X, x1, x2, x);
            BezierFunction curveY = new(state.Y, y1, y2, y);
            for (int t = 0; t < amount; t++)
            {
                double xt = curveX.Calculate(t);
                double yt = curveY.Calculate(t);
                yield return new CommandStatement(new CommandToken(1), new[] { new ArgumentToken("X", xt), new ArgumentToken("Y", yt) });
            }
        }
    }

    public record BezierFunction(double P1, double P2, double P3, double P4)
    {
        public double Calculate(double t)
        {
            double a = (1 - t);
            double a2 = a * a;
            double a3 = a2 * a;
            double t2 = t * t;
            double t3 = t2 * t;
            return a3 * P1 + 3 * a2 * t * P2 + 3 * a * t2 * P3 + t3 * P4;
        }
    }

    public record HorizontalLineToCommand(double X, bool IsRelative = false) : PathCommand
    {
        public override PenState Transform(PenState state) => IsRelative
            ? new(state.X + X, state.Y, 0)
            : new(X, state.Y, 0);
        public override IEnumerable<Statement> GetGCode(PenState state)
        {
            yield return new CommentStatement(new CommentToken(nameof(HorizontalLineToCommand)));
            if (state.Z != 0) yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("Z", 0) }, new CommentToken("Pen Down"));
            yield return new CommandStatement(new CommandToken(1), new[] { new ArgumentToken("X", X) });
        }
    }
    public record VerticalLineToCommand(double Y, bool IsRelative = false) : PathCommand
    {
        public override PenState Transform(PenState state) => IsRelative
            ? new(state.X, state.Y + Y, 0)
            : new(state.X, Y, 0);
        public override IEnumerable<Statement> GetGCode(PenState state)
        {
            yield return new CommentStatement(new CommentToken(nameof(VerticalLineToCommand)));
            if (state.Z != 0) yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("Z", 0) }, new CommentToken("Pen Down"));
            yield return new CommandStatement(new CommandToken(1), new[] { new ArgumentToken("Y", Y) });
        }
    }
    public record MoveToCommand(double X, double Y, bool IsRelative = false) : PathCommand
    {
        public override PenState Transform(PenState state) => IsRelative
            ? new(state.X + X, state.Y + Y, 10)
            : new(X, Y, 10);
        public override IEnumerable<Statement> GetGCode(PenState state)
        {
            yield return new CommentStatement(new CommentToken(nameof(MoveToCommand)));
            if (state.Z != 10) yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("Z", 10) }, new CommentToken("Pen Up"));
            PenState endState = Transform(state);
            yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("X", endState.X), new ArgumentToken("Y", endState.Y) });
        }
    }
    public record LineToCommand(double X, double Y, bool IsRelative = false) : PathCommand
    {
        public override PenState Transform(PenState state) => IsRelative
            ? new(state.X + X, state.Y + Y, 0)
            : new(X, Y, 0);
        public override IEnumerable<Statement> GetGCode(PenState state)
        {
            yield return new CommentStatement(new CommentToken(nameof(MoveToCommand)));
            if (state.Z != 0) yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("Z", 0) }, new CommentToken("Pen Down"));
            PenState endState = Transform(state);
            yield return new CommandStatement(new CommandToken(1), new[] { new ArgumentToken("X", endState.X), new ArgumentToken("Y", endState.Y) });
        }
    }
    public record CloseCommand() : PathCommand;
}
using Microsoft.VisualBasic;
using Svg2Gcode.GCode;
using Svg2Gcode.Svg.Paths;
using Svg2Gcode.Tools;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using static Svg2Gcode.Svg.PathShapeParser;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public override IEnumerable<Segment2D>? Intersect(Segment2D segment)
        {
            // masking for paths is tricky as fill is hard to evaluate
            return null;
        }
        public static PathShape? From(XElement xElement)
        {
            XAttribute? dataAttribute = xElement.Attribute("d");
            if (dataAttribute is null) return null;

            PathShapeParser parser = new();
            return parser.Parse(dataAttribute.Value);
        }
    }

    public class PathShapeFormatter
    {
        public string Format(PathShape shape)
        {
            StringBuilder builder = new();
            string? previousCommand = null;
            foreach (PathCommand command in shape.Commands)
            {
                string nextCommand;
                if (command is LineToCommand lineToCommand) nextCommand = format(lineToCommand);
                else if (command is MoveToCommand moveToCommand) nextCommand = format(moveToCommand);
                else if (command is CubicCurveToCommand cubicCurveToCommand) nextCommand = format(cubicCurveToCommand);
                else if (command is HorizontalLineToCommand horizontalLineToCommand) nextCommand = format(horizontalLineToCommand);
                else if (command is VerticalLineToCommand verticalLineToCommand) nextCommand = format(verticalLineToCommand);
                else if (command is CloseCommand) nextCommand = "z ";
                else throw new Exception();

                bool removeCommandKey = previousCommand is not null && previousCommand[0] == nextCommand[0];
                previousCommand = nextCommand;
                if (removeCommandKey) nextCommand = nextCommand.Substring(2);
                builder.Append(" ");
                builder.Append(nextCommand);
            }
            return builder.ToString();
        }

        private string format(CubicCurveToCommand command)
            => $"{(command.IsRelative ? "c " : "C ")}{command.X1},{command.Y1} {command.X2},{command.Y2} {command.X},{command.Y}";
        private string format(LineToCommand command)
            => $"{(command.IsRelative ? "l " : "L ")}{command.X},{command.Y}";
        private string format(MoveToCommand command)
            => $"{(command.IsRelative ? "m " : "M ")}{command.X},{command.Y}";
        private string format(HorizontalLineToCommand command)
            => $"{(command.IsRelative ? "h " : "H ")}{command.X}";
        private string format(VerticalLineToCommand command)
            => $"{(command.IsRelative ? "v " : "V ")}{command.Y}";
    }

    public class PathShapeParser
    {
        private readonly ICommandParser[] parsers = new ICommandParser[]
        {
            new CommandParser<double, double>('m', (r, a, b) => new MoveToCommand(a, b, r)),
            new CommandParser<double, double>('l', (r, a, b) => new LineToCommand(a, b, r)),
            new CommandParser<double>('h', (r, a) => new HorizontalLineToCommand(a, r)),
            new CommandParser<double>('v', (r, a) => new VerticalLineToCommand(a, r)),
            new CommandParser<double, double, double, double, double, double>('c', (r, a, b, c, d, e, f) => new CubicCurveToCommand(a, b, c, d, e, f, r)),
            new CommandParser('z', r => new CloseCommand())
        };

        public PathShape Parse(string data)
        {
            PathShape path = new();
            path.Commands.AddRange(parse(data));
            return path;
        }
        private IEnumerable<PathCommand> parse(string data)
        {
            bool skipWhiteSpace = false;
            char previousCommandKey = ' ';
            List<string> arguments = new();
            StringBuilder builder = new();

            ICommandParser getParser(char key) => parsers.FirstOrDefault(parser => parser.CanParse(key)) ?? throw new NotSupportedException($"Unsupported {nameof(PathCommand)} of [{key}]");

            for (int i = 0; i < data.Length; i++)
            {
                char commandKey = data[i];
                if (i == 0 && Char.IsLower(commandKey)) commandKey = Char.ToUpper(commandKey); // force first command to be absolute

                if (skipWhiteSpace && commandKey == ' ') continue;
                skipWhiteSpace = false;
                if (Char.IsLetter(commandKey) && commandKey != 'e') // new command
                {
                    if (i > 0) // process the previous command
                    {
                        ICommandParser parser = getParser(previousCommandKey);
                        foreach (PathCommand command in parser.Parse(previousCommandKey, arguments)) yield return command;
                    }

                    previousCommandKey = commandKey;
                    arguments.Clear();
                    skipWhiteSpace = true;
                }
                else if (commandKey == ' ' || commandKey == ',')
                {
                    arguments.Add(builder.ToString());
                    builder.Clear();
                    skipWhiteSpace = true;
                }
                else builder.Append(commandKey);
            }

            // do last 
            {
                ICommandParser parser = getParser(previousCommandKey);
                foreach (PathCommand command in parser.Parse(previousCommandKey, arguments)) yield return command;
            }
        }

        public interface ICommandParser
        {
            public bool CanParse(char key);
            IEnumerable<PathCommand> Parse(char key, List<string> arguments);
        }
        public class CommandParser : BaseCommandParser
        {
            private readonly Func<bool, PathCommand> parser;
            public CommandParser(char key, Func<bool, PathCommand> parser)
                : base(key, 0)
            {
                this.parser = parser;
            }
            protected override PathCommand Parse(bool isRelative, object[] arguments) => parser(isRelative);
        }
        public class CommandParser<T1> : BaseCommandParser
        {
            private readonly Func<bool, T1, PathCommand> parser;
            public CommandParser(char key, Func<bool, T1, PathCommand> parser)
                : base(key, 1)
            {
                this.parser = parser;
            }

            protected override PathCommand Parse(bool isRelative, object[] arguments)
            {
                T1 a1 = (T1)Convert.ChangeType(arguments[0], typeof(T1));
                return parser(isRelative, a1);
            }
        }
        public class CommandParser<T1, T2> : BaseCommandParser
        {
            private readonly Func<bool, T1, T2, PathCommand> parser;
            public CommandParser(char key, Func<bool, T1, T2, PathCommand> parser)
                : base(key, 2)
            {
                this.parser = parser;
            }

            protected override PathCommand Parse(bool isRelative, object[] arguments)
            {
                T1 a1 = (T1)Convert.ChangeType(arguments[0], typeof(T1));
                T2 a2 = (T2)Convert.ChangeType(arguments[1], typeof(T2));
                return parser(isRelative, a1, a2);
            }
        }
        public class CommandParser<T1, T2, T3, T4, T5, T6> : BaseCommandParser
        {
            private readonly Func<bool, T1, T2, T3, T4, T5, T6, PathCommand> parser;
            public CommandParser(char key, Func<bool, T1, T2, T3, T4, T5, T6, PathCommand> parser)
                : base(key, 6)
            {
                this.parser = parser;
            }

            protected override PathCommand Parse(bool isRelative, object[] arguments)
            {
                T1 a1 = (T1)Convert.ChangeType(arguments[0], typeof(T1));
                T2 a2 = (T2)Convert.ChangeType(arguments[1], typeof(T2));
                T3 a3 = (T3)Convert.ChangeType(arguments[2], typeof(T3));
                T4 a4 = (T4)Convert.ChangeType(arguments[3], typeof(T4));
                T5 a5 = (T5)Convert.ChangeType(arguments[4], typeof(T5));
                T6 a6 = (T6)Convert.ChangeType(arguments[5], typeof(T6));
                return parser(isRelative, a1, a2, a3, a4, a5, a6);
            }
        }
        public abstract class BaseCommandParser : ICommandParser
        {
            private readonly char upperCaseKey;
            private readonly char lowerCaseKey;
            private readonly int amountOfArguments;

            public BaseCommandParser(char key, int amountOfArguments)
            {
                upperCaseKey = Char.ToUpper(key);
                lowerCaseKey = Char.ToLower(key);
                this.amountOfArguments = amountOfArguments;
            }

            public bool CanParse(char key) => key == upperCaseKey || key == lowerCaseKey;
            public IEnumerable<PathCommand> Parse(char key, List<string> arguments)
            {
                bool isRelative = key == lowerCaseKey;
                int argumentIndex = 0;
                if (amountOfArguments == 0) yield return Parse(isRelative, new string[0]);
                else
                {
                    while (arguments.Count > argumentIndex)
                    {
                        if (arguments.Count < argumentIndex + amountOfArguments - 1) throw new Exception();
                        object[] args = new object[amountOfArguments];
                        for (int i = 0; i < amountOfArguments; i++) args[i] = arguments[argumentIndex + i];
                        yield return Parse(isRelative, args);
                        argumentIndex += amountOfArguments;
                    }
                }
            }
            protected abstract PathCommand Parse(bool isRelative, object[] arguments);
        }
    }

    public class RectangleShape : Shape
    {
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }
        //public double RX { get; }
        //public double RY { get; }
        public override IEnumerable<Path2D> GetPaths(Vector2D start)
        {
            yield return new Path2D(new Vector2D(X, Y), new Vector2D(X + Width, Y), new Vector2D(X + Width, Y + Height), new Vector2D(X, Y + Height), new Vector2D(X, Y));
        }
    }
}
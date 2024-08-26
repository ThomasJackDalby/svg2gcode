using System.Text;

namespace Svg2Gcode.Svg.Paths
{
    public class PathShapeParser
    {
        private readonly ICommandParser[] parsers = new ICommandParser[]
        {
            new CommandParser<double, double>('m', (r, a, b) => new MoveToCommand(a, b, r)),
            new CommandParser<double, double>('l', (r, a, b) => new LineToCommand(a, b, r)),
            new CommandParser<double>('h', (r, a) => new HorizontalLineToCommand(a, r)),
            new CommandParser<double>('v', (r, a) => new VerticalLineToCommand(a, r)),
            new CommandParser<double, double, double, double, double, double>('c', (r, a, b, c, d, e, f) => new CubicCurveToCommand(a, b, c, d, e, f, r)),
            new CommandParser<double, double, double, double, double, double, double>('a', (r, rX, rY, rot, fS, fA, x, y) => new ArcToCommand(rX, rY, rot, fS, fA, x, y, r)),
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
                if (i == 0 && char.IsLower(commandKey)) commandKey = char.ToUpper(commandKey); // force first command to be absolute

                if (skipWhiteSpace && commandKey == ' ') continue;
                skipWhiteSpace = false;
                if (char.IsLetter(commandKey) && commandKey != 'e') // new command
                {
                    var arg = builder.ToString();
                    if (!string.IsNullOrEmpty(arg))
                        arguments.Add(arg);

                    if (i > 0) // process the previous command
                    {
                        ICommandParser parser = getParser(previousCommandKey);
                        foreach (PathCommand command in parser.Parse(previousCommandKey, arguments)) yield return command;
                    }

                    previousCommandKey = commandKey;
                    arguments.Clear();
                    builder.Clear();
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
                arguments.Add(builder.ToString());

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
        public class CommandParser<T1, T2, T3, T4, T5, T6, T7> : BaseCommandParser
        {
            private readonly Func<bool, T1, T2, T3, T4, T5, T6, T7, PathCommand> parser;
            public CommandParser(char key, Func<bool, T1, T2, T3, T4, T5, T6, T7, PathCommand> parser)
                : base(key, 7)
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
                T7 a7 = (T7)Convert.ChangeType(arguments[6], typeof(T7));
                return parser(isRelative, a1, a2, a3, a4, a5, a6, a7);
            }
        }
        public abstract class BaseCommandParser : ICommandParser
        {
            private readonly char upperCaseKey;
            private readonly char lowerCaseKey;
            private readonly int amountOfArguments;

            public BaseCommandParser(char key, int amountOfArguments)
            {
                upperCaseKey = char.ToUpper(key);
                lowerCaseKey = char.ToLower(key);
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
}
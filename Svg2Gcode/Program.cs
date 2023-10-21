using System.ComponentModel.Design.Serialization;
using System.Text;
using System.Transactions;
using System.Xml.Linq;

namespace Svg2Gcode
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\thoma\Desktop\house.svg";

            // Load SVG document
            SvgDocument? svgDocument = SvgDocument.Load(filePath);
            if (svgDocument is null) return;

            // Compile to gcode
            List<Statement> statements = new();


            foreach (Shape shape in svgDocument.GetShapes()) statements.AddRange(shape.GetGCode());

            // Save out to file
            Save("test.gcode", statements);
        }

        public static void Save(string filePath, IEnumerable<Statement> statements)
        {
            using Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            using StreamWriter writer = new(stream);

            foreach (Statement statement in statements) writer.WriteLine(statement.Text);
        }
    }

    public abstract record Token
    {
        public abstract string Text { get; }
    }
    public record CommandToken(int Number) : Token
    {
        public override string Text => $"G{Number:00}";
    }
    public record ArgumentToken(string Name, double Value) : Token
    {
        public override string Text => $"{Name}{Value:N3}";
    }
    public record CommentToken(string Comment) : Token
    {
        public override string Text => $"({Comment})";
    }
    public abstract record Statement(Token[] Tokens)
    {
        public string Text => String.Join(" ", Tokens.Select(token => token.Text));
    }
    public record CommandStatement(CommandToken Command, ArgumentToken[] Arguments, CommentToken? Comment = null) : Statement(Enumerable.Empty<Token>()
        .Concat(Command)
        .Concat(Arguments)
        .Concat(Comment)
        .WhereNotNull()
        .ToArray());

    public static class LinqExtensions
    {
        public static IEnumerable<T?> Concat<T>(this IEnumerable<T?> items, T? extraItem)
        {
            foreach (T? item in items) yield return item;
            yield return extraItem;
        }
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> items)
        {
            foreach (T item in items)
            {
                if (item is not null) yield return item;
            }
        }
    }

    public class SvgDocument
    {
        public List<Element> Elements { get; } = new();

        public IEnumerable<Shape> GetShapes()
        {
            foreach (Element element in Elements)
            {
                if (element is Shape shape) yield return shape;
                //else if (element is 
            }
        }

        public static SvgDocument? Load(string filePath)
        {
            using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            XDocument? document = XDocument.Load(stream);
            return document is not null ? Load(document) : null;
        }
        public static SvgDocument? Load(XDocument xDocument)
        {
            SvgDocument svgDocument = new();
            if (xDocument.Root is null) return null;
            foreach (XElement xElement in xDocument.Root.Elements())
            {
                Element? element = null;
                if (xElement.Name.LocalName == "path") element = Path.From(xElement);
                // add support for more shapes/groups etc here

                if (element is not null) svgDocument.Elements.Add(element);
            }
            return svgDocument;
        }
    }

    public abstract class Element
    {
    }
    public abstract class Shape : Element
    {
        public abstract IEnumerable<Statement> GetGCode();
    }
    public class Path : Shape
    {
        public List<PathCommand> Commands { get; } = new();

        public static Path? From(XElement xElement)
        {
            Path path = new();

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
                if (Char.IsLetter(c) && c != 'e') // new command
                {
                    static IEnumerable<double[]> getArguments(List<string> arguments, int amount)
                    {
                        int argumentIndex = 0;
                        while (arguments.Count > argumentIndex)
                        {
                            if (arguments.Count < argumentIndex + amount - 1) throw new Exception();
                            double[] args = new double[amount];
                            for (int i = 0; i < amount; i++) args[i] = Double.Parse(arguments[argumentIndex + i]);
                            yield return args;
                            argumentIndex += amount;
                        }
                    }

                    if (i > 0) // process the old command
                    {
                        bool isRelative = Char.IsAsciiLetterLower(command);
                        command = Char.ToLower(command);
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

        public override IEnumerable<Statement> GetGCode() => Commands.SelectMany(command => command.GetGCode());
    }

    public abstract record PathCommand()
    {
        public virtual IEnumerable<Statement> GetGCode(double x, double y, bool isPenUp) => Enumerable.Empty<Statement>();
    }

    public record CurveToCommand(double X1, double Y1, double X2, double Y2, double X, double Y, bool IsRelative = false) : PathCommand;
    public record HorizontalLineToCommand(double X, bool IsRelative = false) : PathCommand;
    public record VerticalLineToCommand(double Y, bool IsRelative = false) : PathCommand;
    public record MoveToCommand(double X, double Y, bool IsRelative = false) : PathCommand
    {
        public override IEnumerable<Statement> GetGCode(double x, double y, bool isPenUp)
        {
            if (!isPenUp) yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("Z", 10) });
            yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("X", X), new ArgumentToken("Y", Y) });
        }

    }
    public record LineToCommand(double X, double Y, bool IsRelative = false) : PathCommand
    {
        public override IEnumerable<Statement> GetGCode(double x, double y, bool isPenUp)
        {
            if (isPenUp) yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("Z", 20) });
            yield return new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("X", X), new ArgumentToken("Y", Y) });
        }
    }
    public record CloseCommand() : PathCommand;


}
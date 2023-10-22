using System.ComponentModel.Design.Serialization;
using System.Transactions;
using System.Xml.Linq;
using Svg2Gcode.GCode;
using Svg2Gcode.Svg;

namespace Svg2Gcode
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\thoma\source\repos\Svg2Gcode\Svg2Gcode\house.svg";

            // Load SVG document
            SvgDocument? svgDocument = SvgDocument.Load(filePath);
            if (svgDocument is null) return;

            // Compile to gcode
            List<Statement> statements = new();

            PenState state = new PenState(0, 0, 10);
            foreach (Shape shape in svgDocument.GetShapes())
            {
                statements.AddRange(shape.GetGCode(state));
                state = shape.Transform(state);
            }

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
                if (xElement.Name.LocalName == "path") element = PathShape.From(xElement);
                // add support for more shapes/groups etc here

                if (element is not null) svgDocument.Elements.Add(element);
            }
            return svgDocument;
        }
    }
}
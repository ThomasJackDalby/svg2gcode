using Dalby.Common.Extensions;
using Dalby.GCode;
using Dalby.GCode.Statements;
using Svg2Gcode.Svg;

namespace Svg2Gcode.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string filePath = "house.svg";

            // Load SVG document
            SvgDocument? svgDocument = SvgDocument.Load(filePath);
            if (svgDocument is null) return;

            // Compile to gcode
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            List<StatementSyntax> statements = GCodeCompiler.Default.Compile(svgDocument).ToList();
            GCodeFormatter.Default.Format(statements).ToFullText().WriteTo($"{fileName}.gcode");
        }

        [Fact]
        public void Test2()
        {
            // Assemble

            SvgDocument svgDocument = new SvgDocument();
            svgDocument.Elements.Add(new RectangleShape { X = 0, Y = 0, Width = 100, Height = 200 });

            // Action
            List<StatementSyntax> statements = GCodeCompiler.Default.Compile(svgDocument).ToList();
            List<StatementSyntax> result = GCodeFormatter.Default.Format(statements).ToList();
            
            // Assert
            string name = "test2";
            result.ToFullText().WriteTo($"{name}.gcode");
            result.ToSyntaxTree($"{name}.syntax.txt");
        }


        [Fact]
        public void Test3()
        {
            // Assemble
            SvgDocument svgDocument = new SvgDocument();
            svgDocument.Elements.Add(new RectangleShape { X = 0, Y = 0, Width = 100, Height = 100 });
            svgDocument.Elements.Add(new RectangleShape { X = 100, Y = 100, Width = 100, Height = 100 });


        }
    }
}
using Svg2Gcode.GCode;

namespace Svg2Gcode
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "test.svg";

            // Load SVG document
            SvgDocument? svgDocument = SvgDocument.Load(filePath);
            if (svgDocument is null) return;

            
            // Compile to gcode
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            GCodeCompiler compiler = new();
            compiler.Compile($"{fileName}.gcode", svgDocument);
        }
    }
}

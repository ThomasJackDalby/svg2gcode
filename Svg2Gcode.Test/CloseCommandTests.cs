using Dalby.Common.Extensions;
using Dalby.GCode;
using Dalby.GCode.Statements;
using Svg2Gcode.Svg;
using Svg2Gcode.Svg.Paths;

namespace Svg2Gcode.Test
{
    public class CloseCommandTests
    {
        [Theory]
        [InlineData(0, 0, 10, 20, true)]
        [InlineData(0, 0, 10, 20, false)]
        [InlineData(5, 10, 10, 20, true)]
        [InlineData(5, 10, 10, 20, false)]
        public void ConfirmCloseCommandDoesNotAddSuperfluousPoint(double sX, double sY, double dX, double dY, bool pathAlreadyClosed)
        {
            // Assemble
            PathShape shape = new();
            shape.Commands.Add(new MoveToCommand(sX, sY));
            shape.Commands.Add(new LineToCommand(dX, 0, true));
            shape.Commands.Add(new LineToCommand(0, dY, true));
            shape.Commands.Add(new LineToCommand(-dX, 0, true));
            if (pathAlreadyClosed) shape.Commands.Add(new LineToCommand(0, -dY, true));
            shape.Commands.Add(new CloseCommand());

            // Action
            SvgDocument svgDocument = new();
            svgDocument.Elements.Add(shape);
            StatementSyntax[] statements = GCodeCompiler.Default.Compile(svgDocument).ToArray();
            GCodeFormatter.Default.Format(statements).ToFullText().WriteTo($"close-command-test-{sX}-{sY}-{dX}-{dY}-{pathAlreadyClosed}.gcode");

            // Assert
            // Confirm no adjecant G01 commands are the same (i.e. superfluous).
            for (int i = 1; i < statements.Length; i++)
            {
                StatementSyntax previous = statements[i-1];
                StatementSyntax current = statements[i];
                if (previous is CommandStatementSyntax previousCommand 
                    && current is CommandStatementSyntax currentCommand
                    && previousCommand.Command.Text == "G01"
                    && currentCommand.Command.Text == "G01")
                {
                    Assert.NotEqual(previousCommand.FullText, currentCommand.FullText);
                }
            }
        }
    }
}

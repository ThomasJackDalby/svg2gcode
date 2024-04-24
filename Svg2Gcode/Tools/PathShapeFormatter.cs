using Svg2Gcode.Svg;
using Svg2Gcode.Svg.Paths;
using System.Text;

namespace Svg2Gcode.Tools
{
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
}
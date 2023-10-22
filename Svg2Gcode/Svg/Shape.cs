using Svg2Gcode.GCode;
using static Svg2Gcode.Svg.PathShape;

namespace Svg2Gcode.Svg
{
    public abstract class Shape : Element
    {
        public abstract PenState Transform(PenState state);
        public abstract IEnumerable<Statement> GetGCode(PenState state);
    }

    //public abstract class CommandParser
    //{ }

    //public class MoveUpCommandParser : CommandParser
    //{
    //    public bool

    //}
}
using GCode.Statements;
using GCode.Trivias;
using Svg2Gcode.GCode;

namespace Dalby.GCode
{
    public static class Commands
    {
        public static CommandStatementSyntax G0_RapidMotion(double? x = null, double? y = null, double? z = null)
        {
            List<ArgumentToken> arguments = new();
            if (x.HasValue) arguments.Add(new ArgumentToken('X', x.Value));
            if (y.HasValue) arguments.Add(new ArgumentToken('Y', y.Value));
            if (z.HasValue) arguments.Add(new ArgumentToken('Z', z.Value));
            return new CommandStatementSyntax(new CommandToken('G', 0), arguments.ToArray())
                .WithTrailingTrivia(new EndOfLineTrivia());
        }
        public static CommandStatementSyntax G1_CoordinatedMotion(double? x = null, double? y = null, double? z = null, double? f = null)
        {
            List<ArgumentToken> arguments = new();
            if (x.HasValue) arguments.Add(new ArgumentToken('X', x.Value));
            if (y.HasValue) arguments.Add(new ArgumentToken('Y', y.Value));
            if (z.HasValue) arguments.Add(new ArgumentToken('Z', z.Value));
            if (f.HasValue) arguments.Add(new ArgumentToken('F', f.Value));
            return new CommandStatementSyntax(new CommandToken('G', 1), arguments.ToArray())
                .WithTrailingTrivia(new EndOfLineTrivia());
        }

        public static CommandStatementSyntax G2_ClockwiseArcMotion() => G(21);
        public static CommandStatementSyntax G3_CounterClockwiseArcMotion() => G(21);

        public enum TimeUnits
        {
            MilliSeconds,
            Seconds,
        }
        public static CommandStatementSyntax G4_Pause(double duration, TimeUnits units)
        {
            char key = units switch
            {
                TimeUnits.MilliSeconds => 'P',
                TimeUnits.Seconds => 'S',
                _ => throw new NotSupportedException(nameof(units))
            };
            return G(21).A(key, duration);
        }
        public static CommandStatementSyntax G20_Inches() => G(20);
        public static CommandStatementSyntax G21_Millimetres() => G(21);
        public static CommandStatementSyntax G90_AbsoluteCoordinates() => G(90);
        public static CommandStatementSyntax G17_XYPlane() => G(17);
        public static CommandStatementSyntax G94_UnitsPerMinuteFeedRateMode() => G(94);
        public static CommandStatementSyntax M3_TurnOnSpindle(int speed) => M(3).A('S', 1000);


        public static Builder G(int code) => new('G', code);
        public static Builder M(int code) => new('M', code);
        public class Builder
        {
            private readonly CommandToken command;
            private readonly List<ArgumentToken> arguments = new();

            public Builder(char key, int code)
            {
                command = new CommandToken(key, code);
            }
            public Builder A(char name, double value)
            {
                arguments.Add(new(name, value));
                return this;
            }

            public static implicit operator CommandStatementSyntax(Builder builder)
                => new CommandStatementSyntax(builder.command, builder.arguments.ToArray()).WithTrailingTrivia(new EndOfLineTrivia());
        }
    }
}

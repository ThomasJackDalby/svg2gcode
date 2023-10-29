namespace Svg2Gcode.GCode
{
    public static class Commands
    {
        public static CommandStatement G0_RapidMotion(double? x = null, double? y = null, double? z = null)
        {
            List<ArgumentToken> arguments = new();
            if (x.HasValue) arguments.Add(new ArgumentToken('X', x.Value));
            if (y.HasValue) arguments.Add(new ArgumentToken('Y', y.Value));
            if (z.HasValue) arguments.Add(new ArgumentToken('Z', z.Value));
            return new CommandStatement(new CommandToken('G', 0), arguments.ToArray());
        }
        public static CommandStatement G1_CoordinatedMotion(double? x = null, double? y = null, double? z = null, double? f = null)
        {
            List<ArgumentToken> arguments = new();
            if (x.HasValue) arguments.Add(new ArgumentToken('X', x.Value));
            if (y.HasValue) arguments.Add(new ArgumentToken('Y', y.Value));
            if (z.HasValue) arguments.Add(new ArgumentToken('Z', z.Value));
            if (f.HasValue) arguments.Add(new ArgumentToken('F', f.Value));
            return new CommandStatement(new CommandToken('G', 1), arguments.ToArray());
        }

        public static CommandStatement G2_ClockwiseArcMotion() => G(21);
        public static CommandStatement G3_CounterClockwiseArcMotion() => G(21);
        
        public enum TimeUnits
        {
            MilliSeconds,
            Seconds,
        }
        public static CommandStatement G4_Pause(double duration, TimeUnits units)
        {
            char key = units switch
            {
                TimeUnits.MilliSeconds => 'P',
                TimeUnits.Seconds => 'S',
                _ => throw new NotSupportedException(nameof(units))
            };
            return G(21).A(key, duration);
        }
        public static CommandStatement G20_Inches() => G(20);
        public static CommandStatement G21_Millimetres() => G(21);
        public static CommandStatement G90_AbsoluteCoordinates() => G(90);
        public static CommandStatement G17_XYPlane() => G(17);
        public static CommandStatement G94_UnitsPerMinuteFeedRateMode() => G(94);
        public static CommandStatement M3_TurnOnSpindle(int speed) => M(3).A('S', 1000);
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

            public static implicit operator CommandStatement(Builder builder) 
                => new(builder.command, builder.arguments.ToArray());
        }
    }
}

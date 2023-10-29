namespace Svg2Gcode.Tools
{
    public record CubicBezierFunction(double P1, double P2, double P3, double P4)
    {
        public double GetApproximateLength()
        {
            double chordLength = P4 - P1;
            double polygonLength = Math.Abs(P2 - P1) + Math.Abs(P3 - P2) + Math.Abs(P4 - P3);
            return (2 * chordLength + polygonLength) / 3.0;
        }
        public double Calculate(double t)
        {
            double a = 1 - t;
            double a2 = a * a;
            double a3 = a2 * a;
            double t2 = t * t;
            double t3 = t2 * t;
            return a3 * P1 + 3 * a2 * t * P2 + 3 * a * t2 * P3 + t3 * P4;
        }
    }
}
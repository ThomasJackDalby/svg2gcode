using Svg2Gcode.Spatial;
using Svg2Gcode.Tools;
using Utils.Spatial;

namespace Svg2Gcode.Svg.Paths
{
    public record CubicCurveToCommand(double X1, double Y1, double X2, double Y2, double X, double Y, bool IsRelative = false) : PathCommand(IsRelative)
    {
        public override IEnumerable<(bool, Path2D)> GetPaths(Vector2D commandStart, Vector2D start)
        {
            (double x1, double y1, double x2, double y2, double x, double y) = IsRelative
                ? (start.X + X1, start.Y + Y1, start.X + X2, start.Y + Y2, start.X + X, start.Y + Y)
                : (X1, Y1, X2, Y2, X, Y);

            int amount = 10;
            double dt = 1.0 / (amount - 1);
            CubicBezierFunction curveX = new(start.X, x1, x2, x);
            CubicBezierFunction curveY = new(start.Y, y1, y2, y);
            Vector2D[] points = new Vector2D[amount];
            points[0] = start;
            for (int ti = 1; ti < amount; ti++)
            {
                double t = ti * dt;
                double xt = curveX.Calculate(t);
                double yt = curveY.Calculate(t);
                points[ti] = new Vector2D(xt, yt);
            }
            yield return (true, new Path2D(points));
        }
    }
}
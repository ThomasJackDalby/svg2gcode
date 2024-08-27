using Dalby.Maths.Geometry;

namespace Svg2Gcode.Svg.Paths
{
    public record ArcToCommand : PathCommand
    {
        public double RadiusX { get; }
        public double RadiusY { get; }
        public double Rotation { get; }
        public double LargeArcFlag { get; }
        public double SweepFlag { get; }
        public double X { get; }
        public double Y { get; }

        public ArcToCommand(double radiusX, double radiusY, double rotation, double largeArcFlag, double sweepFlag, double x, double y, bool isRelative = false)
            : base(isRelative)
        {
            RadiusX = radiusX;
            RadiusY = radiusY;
            Rotation = rotation;
            LargeArcFlag = largeArcFlag;
            SweepFlag = sweepFlag;
            X = x;
            Y = y;
        }

        public override IEnumerable<(bool, Path2D)> GetPaths(Vector2D commandStart, Vector2D start)
        {
            Arc arc = Arc.From(start, new(X, Y), new(RadiusX, RadiusY), Rotation, LargeArcFlag == 1, SweepFlag == 1);
            Vector2D[] points = arc.GetPoints(25);
            yield return (true, new(points));
        }

        private static double angle(Vector2D a, Vector2D b)
        {
            double sign = a.X * b.Y - a.Y * b.X;
            double acos = Math.Acos(a.Dot(b) / (a.Abs() * b.Abs()));
            return Math.CopySign(acos, sign);
        }

        public record Arc
        {
            public double Rotation { get; }
            public Vector2D Center { get; }
            public Vector2D Radius { get; }
            public double StartAngle { get; }
            public double EndAngle { get; }

            private readonly double sinPsi;
            private readonly double cosPsi;

            public Arc(Vector2D center, Vector2D radius, double rotation, double startAngle, double endAngle)
            {
                Center = center;
                Radius = radius;
                Rotation = rotation;
                StartAngle = startAngle;
                EndAngle = endAngle;
                cosPsi = Math.Cos(Rotation);
                sinPsi = Math.Sin(Rotation);
            }

            public Vector2D GetPoint(double theta)
            {
                double rxCosTheta = Radius.X * Math.Cos(theta);
                double rySinTheta = Radius.Y * Math.Sin(theta);
                double x = cosPsi * rxCosTheta - sinPsi * rySinTheta + Center.X;
                double y = sinPsi * rxCosTheta + cosPsi * rySinTheta + Center.Y;
                return new(x, y);
            }
            public Vector2D[] GetPoints(int numberOfPoints)
            {
                Vector2D[] points = new Vector2D[numberOfPoints];
                double deltaAngle = (EndAngle - StartAngle) / (points.Length - 1);
                
                for (int i = 0; i < points.Length; i++)
                {
                    double angle = StartAngle + deltaAngle * i;
                    points[i] = GetPoint(angle);
                }
                return points;
            }

            public static Arc From(Vector2D start, Vector2D end, Vector2D radius, double rotation, bool largeArcFlag, bool sweepFlag)
            {
                // https://www.w3.org/TR/SVG/implnote.html
                double x1 = start.X;
                double y1 = start.Y;
                double x2 = end.X;
                double y2 = end.Y;
                double cosPsi = Math.Cos(rotation);
                double sinPsi = Math.Sin(rotation);

                // step 1
                double dX = (x1 - x2) / 2;
                double dY = (y1 - y2) / 2;

                double x1P = cosPsi * dX + sinPsi * dY;
                double y1P = -sinPsi * dX + cosPsi * dY;

                // step 2
                double rX = radius.X;
                double rY = radius.Y;
                double rX2 = radius.X * radius.X;
                double rY2 = radius.Y * radius.Y;
                double a = rX2 * rY2 - rX2 * y1P * y1P - rY2 * x1P * x1P;
                double b = rX2 * y1P * y1P + rY2 * x1P * x1P;
                double c = Math.Sqrt(a / b);
                if (largeArcFlag == sweepFlag) c = -c;

                double cXP = c * rX * y1P / rY;
                double cYP = c * -rY * x1P / rX;

                // step 3
                double cX = cosPsi * cXP - sinPsi * cYP + (x1 + x2) / 2;
                double cY = sinPsi * cXP + cosPsi * cYP + (y1 + y2) / 2;

                Vector2D d1 = new((x1P - cXP) / rX, (y1P - cYP) / rY);
                Vector2D d2 = new((-x1P - cXP) / rX, (-y1P - cYP) / rY);
                double startAngle = angle(new(1, 0), d1);
                double dAngle = angle(d1, d2);

                if (!sweepFlag && dAngle > 0) dAngle -= Math.PI * 2;
                else if (sweepFlag && dAngle < 0) dAngle += Math.PI * 2;
  
                double endAngle = startAngle + dAngle;

                return new Arc(new(cX, cY), radius, rotation, startAngle, endAngle);
            }
        }
    }
}
namespace Svg2Gcode.Tools
{
    public record Vector2D(double X, double Y);

    public record Segment2D(Vector2D Start, Vector2D End)
    {
        public Limits2D Limits => limits ??= Limits2D.From(Start, End);

        private Limits2D? limits = null;

        public Segment2D? Intersect(Segment2D other)
        {
            return null;
        }
    }

    public record Limits2D(Vector2D Min, Vector2D Max) 
    {
    
        public static Limits2D From(params Vector2D[] points)
        {
            double minX = points.Select(p => p.X).Min();
            double minY = points.Select(p => p.Y).Min();
            double maxX = points.Select(p => p.X).Max();
            double maxY = points.Select(p => p.Y).Max();
            return new Limits2D(new Vector2D(minX, minY), new Vector2D(maxX, maxY));
        }
    }
}
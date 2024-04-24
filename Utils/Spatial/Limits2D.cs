using Svg2Gcode.Spatial;

namespace Utils.Spatial
{
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
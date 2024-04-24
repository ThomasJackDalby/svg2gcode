using Utils.Spatial;

namespace Svg2Gcode.Spatial
{
    public record Path2D(params Vector2D[] Points)
    {
        public IEnumerable<Segment2D> ToSegments()
        {
            Vector2D? previous = Points.FirstOrDefault();
            if (previous is null) yield break;
            foreach (Vector2D point in Points.Skip(1))
            {
                yield return new Segment2D(point, previous);
                previous = point;
            }
        }
    }
}
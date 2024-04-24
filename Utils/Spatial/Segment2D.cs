namespace Utils.Spatial
{
    public record Segment2D(Vector2D Start, Vector2D End)
    {
        public Limits2D Limits => limits ??= Limits2D.From(Start, End);

        private Limits2D? limits = null;

        public bool Intersects(Segment2D other)
        {
            Vector2D a = Start;
            Vector2D b = End;
            Vector2D c = other.Start;
            Vector2D d = other.End;

            double h(Vector2D p) => (b - a).Cross(p - a);
            double hC = h(c);
            double hD = h(d);

            if (hC == 0 && hD == 0)
            {
                return Math.Min(c.X, d.X) <= Math.Max(a.X, b.X) && Math.Max(c.X, d.X) >= Math.Min(a.X, b.X)
                    && Math.Min(c.Y, d.Y) <= Math.Max(a.Y, b.Y) && Math.Max(c.Y, d.Y) >= Math.Min(a.Y, b.Y);
            }

            double g(Vector2D p) => (d - c).Cross(p - c);
            double gA = g(a);
            double gB = g(b);
            return hC * hD <= 0 && gA * gB <= 0;
        }

        public Vector2D? Intersect(Segment2D other)
        {
            if (!Intersects(other)) return null;
            Vector2D a = Start;
            Vector2D b = End;
            Vector2D c = other.Start;
            Vector2D d = other.End;

            double t = (c - a).Cross(d - c) / (d - c).Cross(b - a);
            return (b - a) * t + a;
        }
    }
}
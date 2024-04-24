namespace Utils.Extensions
{
    //public static class GCodeCommandBuilder
    //{
    //    public static CommandStatement GO_RapidMovement(double x, double y) => new CommandStatement(new CommandToken(0), new[] { new ArgumentToken("X", x), new ArgumentToken("Y", y) });
    //    public static CommandStatement G1_CoordinatedMovement(double x, double y) => new CommandStatement(new CommandToken(1), new[] { new ArgumentToken("X", x), new ArgumentToken("Y", y) });
    //}

    public static class LinqExtensions
    {
        public static IEnumerable<T?> Concat<T>(this IEnumerable<T?> items, T? extraItem)
        {
            foreach (T? item in items) yield return item;
            yield return extraItem;
        }
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> items)
        {
            foreach (T? item in items)
            {
                if (item is not null) yield return item;
            }
        }
    }
}
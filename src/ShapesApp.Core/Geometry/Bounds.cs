namespace ShapesApp.Core.Geometry;

/// <summary>Axis-aligned rectangle in canvas coordinates (Y grows downwards).</summary>
public readonly record struct Bounds(double X, double Y, double Width, double Height)
{
    public double Left => X;

    public double Top => Y;

    public double Right => X + Width;

    public double Bottom => Y + Height;

    public IEnumerable<Point2D> Corners
    {
        get
        {
            yield return new Point2D(Left, Top);
            yield return new Point2D(Right, Top);
            yield return new Point2D(Right, Bottom);
            yield return new Point2D(Left, Bottom);
        }
    }

    public static Bounds FromEdges(double left, double top, double right, double bottom) =>
        new(left, top, right - left, bottom - top);

    /// <summary>Whether <paramref name="other"/> lies completely inside this rectangle (touching edges counts as inside).</summary>
    public bool Contains(Bounds other) =>
        other.Left >= Left && other.Right <= Right && other.Top >= Top && other.Bottom <= Bottom;

    public Bounds Union(Bounds other) => FromEdges(
        Math.Min(Left, other.Left),
        Math.Min(Top, other.Top),
        Math.Max(Right, other.Right),
        Math.Max(Bottom, other.Bottom));

    public override string ToString() => $"({Left:0}, {Top:0}) – ({Right:0}, {Bottom:0})";
}

public readonly record struct Point2D(double X, double Y)
{
    public override string ToString() => $"({X:0}, {Y:0})";
}

using ShapesApp.Core.Geometry;

namespace ShapesApp.Core.Services;

/// <summary>Creates random rectangles around the main area. Some of them stick out of it on purpose.</summary>
public sealed class ShapeGenerator(Random random)
{
    public const double MinSize = 20;
    public const double MaxSize = 150;

    /// <summary>Distinct, pleasant hues: colors are spread around the color wheel instead of being fully random.</summary>
    private const double GoldenAngle = 137.508;

    public ShapeGenerator()
        : this(Random.Shared)
    {
    }

    /// <summary>
    /// Places the top-left corner of every rectangle inside <paramref name="mainArea"/> and gives it a random size,
    /// keeping it within <paramref name="canvas"/>.
    /// </summary>
    public IReadOnlyList<RectangleShape> Generate(int count, Bounds mainArea, Bounds canvas)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (!canvas.Contains(mainArea))
        {
            throw new ArgumentException("The main area must lie inside the canvas.", nameof(mainArea));
        }

        var shapes = new List<RectangleShape>(count);
        var hue = random.NextDouble() * 360;

        for (var i = 0; i < count; i++)
        {
            var x = Next(mainArea.Left, mainArea.Right - MinSize);
            var y = Next(mainArea.Top, mainArea.Bottom - MinSize);
            var width = Math.Min(Next(MinSize, MaxSize), canvas.Right - x);
            var height = Math.Min(Next(MinSize, MaxSize), canvas.Bottom - y);

            var color = ShapeColor.FromHsv(hue, Next(0.55, 0.8), Next(0.75, 0.95));
            hue += GoldenAngle;

            shapes.Add(new RectangleShape(i + 1, new Bounds(x, y, width, height), color));
        }

        return shapes;
    }

    private double Next(double min, double max) => Math.Round(min + random.NextDouble() * (max - min));
}

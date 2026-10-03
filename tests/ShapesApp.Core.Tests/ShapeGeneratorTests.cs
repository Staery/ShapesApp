using ShapesApp.Core.Geometry;
using ShapesApp.Core.Services;

namespace ShapesApp.Core.Tests;

public class ShapeGeneratorTests
{
    private static readonly Bounds Canvas = new(0, 0, 800, 520);

    [Fact]
    public void Generate_PlacesShapesAroundMainAreaInsideCanvas()
    {
        var shapes = new ShapeGenerator(new Random(42)).Generate(500, Shapes.Area, Canvas);

        Assert.Equal(500, shapes.Count);
        Assert.Equal(Enumerable.Range(1, 500), shapes.Select(shape => shape.Number));
        Assert.All(shapes, shape =>
        {
            Assert.InRange(shape.Bounds.Left, Shapes.Area.Left, Shapes.Area.Right);
            Assert.InRange(shape.Bounds.Top, Shapes.Area.Top, Shapes.Area.Bottom);
            Assert.InRange(shape.Bounds.Width, 1, ShapeGenerator.MaxSize);
            Assert.InRange(shape.Bounds.Height, 1, ShapeGenerator.MaxSize);
            Assert.True(Canvas.Contains(shape.Bounds));
        });

        // Some rectangles must stick out, otherwise "exclude outliers" would be pointless.
        Assert.Contains(shapes, shape => !Shapes.Area.Contains(shape.Bounds));
        Assert.Contains(shapes, shape => Shapes.Area.Contains(shape.Bounds));
    }

    [Fact]
    public void Generate_IsReproducibleWithSameSeed()
    {
        var first = new ShapeGenerator(new Random(7)).Generate(10, Shapes.Area, Canvas);
        var second = new ShapeGenerator(new Random(7)).Generate(10, Shapes.Area, Canvas);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Generate_UsesDistinctColors()
    {
        var shapes = new ShapeGenerator(new Random(1)).Generate(12, Shapes.Area, Canvas);

        Assert.Equal(12, shapes.Select(shape => shape.Color).Distinct().Count());
    }

    [Fact]
    public void Generate_RejectsAreaOutsideCanvas() =>
        Assert.Throws<ArgumentException>(() => new ShapeGenerator().Generate(1, new Bounds(700, 0, 200, 100), Canvas));
}

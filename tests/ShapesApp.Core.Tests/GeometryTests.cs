using ShapesApp.Core.Geometry;

namespace ShapesApp.Core.Tests;

public class GeometryTests
{
    [Fact]
    public void Bounds_ExposesEdgesAndCorners()
    {
        var bounds = new Bounds(10, 20, 30, 40);

        Assert.Equal((10, 20, 40, 60), (bounds.Left, bounds.Top, bounds.Right, bounds.Bottom));
        Assert.Equal(
            [new Point2D(10, 20), new Point2D(40, 20), new Point2D(40, 60), new Point2D(10, 60)],
            bounds.Corners);
    }

    [Theory]
    [InlineData(10, 10, 20, 20, true)]
    [InlineData(0, 0, 100, 100, true)]
    [InlineData(90, 10, 20, 20, false)]
    [InlineData(-1, 10, 20, 20, false)]
    public void Bounds_Contains(double x, double y, double width, double height, bool expected) =>
        Assert.Equal(expected, new Bounds(0, 0, 100, 100).Contains(new Bounds(x, y, width, height)));

    [Fact]
    public void Bounds_Union()
    {
        var union = new Bounds(0, 10, 10, 10).Union(new Bounds(20, 0, 5, 5));

        Assert.Equal(Bounds.FromEdges(0, 0, 25, 20), union);
    }

    [Theory]
    [InlineData("#FF8000", 255, 128, 0)]
    [InlineData("00ff7f", 0, 255, 127)]
    public void Color_ParseAndHexRoundTrip(string hex, byte r, byte g, byte b)
    {
        var color = ShapeColor.Parse(hex);

        Assert.Equal(new ShapeColor(r, g, b), color);
        Assert.Equal(ShapeColor.Parse(color.Hex), color);
    }

    [Theory]
    [InlineData("#12345")]
    [InlineData("#GGGGGG")]
    public void Color_RejectsInvalidHex(string hex) =>
        Assert.Throws<FormatException>(() => ShapeColor.Parse(hex));

    [Fact]
    public void Color_RejectsEmptyText() =>
        Assert.ThrowsAny<ArgumentException>(() => ShapeColor.Parse(" "));

    [Theory]
    [InlineData(0, 1, 1, "#FF0000")]
    [InlineData(120, 1, 1, "#00FF00")]
    [InlineData(240, 1, 1, "#0000FF")]
    [InlineData(480, 1, 1, "#00FF00")]
    [InlineData(0, 0, 0.5, "#808080")]
    public void Color_FromHsv(double hue, double saturation, double value, string expected) =>
        Assert.Equal(expected, ShapeColor.FromHsv(hue, saturation, value).Hex);
}

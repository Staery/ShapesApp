using ShapesApp.Core.Geometry;
using ShapesApp.Core.Services;

namespace ShapesApp.Core.Tests;

public class ExtremePointsCalculatorTests
{
    private static readonly RectangleShape InsideRed = Shapes.Rect(1, 150, 150, 50, 50, "#FF0000");
    private static readonly RectangleShape InsideBlue = Shapes.Rect(2, 300, 200, 100, 40, "#0000FF");
    private static readonly RectangleShape OutlierGreen = Shapes.Rect(3, 550, 300, 120, 120, "#00FF00");
    private static readonly RectangleShape[] All = [InsideRed, InsideBlue, OutlierGreen];

    private static ExtremePointsOptions Options(bool excludeOutliers, ColorFilterMode mode = ColorFilterMode.Off, params string[] colors) =>
        new(excludeOutliers, mode, colors.Select(ShapeColor.Parse).ToHashSet());

    [Fact]
    public void ExcludingOutliers_UsesOnlyShapesInsideMainArea()
    {
        var result = ExtremePointsCalculator.Calculate(All, Shapes.Area, Options(excludeOutliers: true));

        Assert.Equal([InsideRed, InsideBlue], result.Included);
        Assert.Equal(Bounds.FromEdges(150, 150, 400, 240), result.Box);
    }

    [Fact]
    public void IncludingOutliers_ExtendsBoxBeyondMainArea()
    {
        var result = ExtremePointsCalculator.Calculate(All, Shapes.Area, Options(excludeOutliers: false));

        Assert.Equal(3, result.Included.Count);
        Assert.Equal(Bounds.FromEdges(150, 150, 670, 420), result.Box);
    }

    [Fact]
    public void OnlySelectedColors()
    {
        var result = ExtremePointsCalculator.Calculate(All, Shapes.Area, Options(false, ColorFilterMode.OnlySelected, "#0000FF", "#00FF00"));

        Assert.Equal([InsideBlue, OutlierGreen], result.Included);
        Assert.Equal(Bounds.FromEdges(300, 200, 670, 420), result.Box);
    }

    [Fact]
    public void ExceptSelectedColors()
    {
        var result = ExtremePointsCalculator.Calculate(All, Shapes.Area, Options(true, ColorFilterMode.ExceptSelected, "#FF0000"));

        Assert.Equal([InsideBlue], result.Included);
        Assert.Equal(InsideBlue.Bounds, result.Box);
    }

    [Theory]
    [InlineData(ColorFilterMode.OnlySelected)]
    [InlineData(ColorFilterMode.ExceptSelected)]
    public void ColorFilterWithoutSelection_HasNoEffect(ColorFilterMode mode)
    {
        var result = ExtremePointsCalculator.Calculate(All, Shapes.Area, Options(false, mode));

        Assert.Equal(3, result.Included.Count);
    }

    [Fact]
    public void SelectedColorsAreIgnoredWhenFilterIsOff()
    {
        var result = ExtremePointsCalculator.Calculate(All, Shapes.Area, Options(false, ColorFilterMode.Off, "#FF0000"));

        Assert.Equal(3, result.Included.Count);
    }

    [Fact]
    public void NoMatch_ReturnsNoBoxInsteadOfInfiniteRectangle()
    {
        var result = ExtremePointsCalculator.Calculate([OutlierGreen], Shapes.Area, Options(excludeOutliers: true));

        Assert.Null(result.Box);
        Assert.Empty(result.Included);
    }

    [Fact]
    public void ShapeTouchingMainAreaEdge_IsNotAnOutlier()
    {
        var touching = Shapes.Rect(4, 100, 100, 500, 250);

        Assert.False(ExtremePointsCalculator.IsOutlier(touching, Shapes.Area));
    }
}

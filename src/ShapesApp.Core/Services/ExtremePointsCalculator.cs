using ShapesApp.Core.Geometry;

namespace ShapesApp.Core.Services;

public enum ColorFilterMode
{
    /// <summary>Colors are not taken into account.</summary>
    Off,

    /// <summary>Only rectangles of the selected colors are used.</summary>
    OnlySelected,

    /// <summary>Rectangles of the selected colors are skipped.</summary>
    ExceptSelected,
}

/// <summary>Which rectangles take part in the calculation.</summary>
public sealed record ExtremePointsOptions(
    bool ExcludeOutliers,
    ColorFilterMode ColorFilter,
    IReadOnlySet<ShapeColor> SelectedColors)
{
    public static ExtremePointsOptions Default { get; } = new(true, ColorFilterMode.Off, new HashSet<ShapeColor>());
}

/// <param name="Box">Bounding box of all extreme points, or <see langword="null"/> if no rectangle matched.</param>
/// <param name="Included">The rectangles that were taken into account.</param>
public sealed record ExtremePointsResult(Bounds? Box, IReadOnlyList<RectangleShape> Included)
{
    public static ExtremePointsResult Empty { get; } = new(null, []);
}

/// <summary>Finds the extreme (left-, right-, top- and bottom-most) points of a filtered set of rectangles.</summary>
public static class ExtremePointsCalculator
{
    public static ExtremePointsResult Calculate(
        IEnumerable<RectangleShape> shapes,
        Bounds mainArea,
        ExtremePointsOptions options)
    {
        ArgumentNullException.ThrowIfNull(shapes);
        ArgumentNullException.ThrowIfNull(options);

        var included = shapes.Where(shape => IsIncluded(shape, mainArea, options)).ToList();
        if (included.Count == 0)
        {
            return ExtremePointsResult.Empty;
        }

        var box = included.Select(shape => shape.Bounds).Aggregate((a, b) => a.Union(b));
        return new ExtremePointsResult(box, included);
    }

    public static bool IsOutlier(RectangleShape shape, Bounds mainArea) => !mainArea.Contains(shape.Bounds);

    public static bool IsIncluded(RectangleShape shape, Bounds mainArea, ExtremePointsOptions options)
    {
        if (options.ExcludeOutliers && IsOutlier(shape, mainArea))
        {
            return false;
        }

        // With nothing selected the color filter has no effect, whichever mode is chosen.
        if (options.SelectedColors.Count == 0)
        {
            return true;
        }

        return options.ColorFilter switch
        {
            ColorFilterMode.OnlySelected => options.SelectedColors.Contains(shape.Color),
            ColorFilterMode.ExceptSelected => !options.SelectedColors.Contains(shape.Color),
            _ => true,
        };
    }
}

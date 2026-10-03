using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShapesApp.Core.Geometry;
using ShapesApp.Core.Services;

namespace ShapesApp.Core.ViewModels;

/// <summary>Generates rectangles and highlights the extreme points of the ones that pass the filters.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    public const int MinShapeCount = 1;
    public const int MaxShapeCount = 30;

    private readonly ShapeGenerator _generator;
    private readonly IActivityLog _log;

    [ObservableProperty]
    private int _shapeCount = 6;

    [ObservableProperty]
    private bool _excludeOutliers = true;

    [ObservableProperty]
    private ColorFilterOption _selectedColorFilter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BoundingBoxText), nameof(IsBoxShown), nameof(ExtremeCorners))]
    private bool _isBoundingBoxVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBoundingBox), nameof(BoundingBoxText), nameof(IsBoxShown), nameof(Box), nameof(ExtremeCorners))]
    private Bounds? _boundingBox;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryText))]
    private int _includedCount;

    public MainViewModel(ShapeGenerator generator, IActivityLog log)
    {
        _generator = generator;
        _log = log;

        ColorFilters =
        [
            new ColorFilterOption("Ignore colors", ColorFilterMode.Off),
            new ColorFilterOption("Only selected", ColorFilterMode.OnlySelected),
            new ColorFilterOption("All except selected", ColorFilterMode.ExceptSelected),
        ];
        _selectedColorFilter = ColorFilters[0];

        Generate();
    }

    /// <summary>Size of the drawing surface.</summary>
    public static Bounds Canvas { get; } = new(0, 0, 800, 520);

    /// <summary>The main rectangle: shapes are generated around it, and those not fully inside it are outliers.</summary>
    public static Bounds MainArea { get; } = new(100, 100, 500, 250);

    public IReadOnlyList<ColorFilterOption> ColorFilters { get; }

    public ObservableCollection<ShapeItemViewModel> Shapes { get; } = [];

    public ObservableCollection<ColorOptionViewModel> Colors { get; } = [];

    /// <summary>Instance accessors for data binding.</summary>
    public Bounds CanvasBounds => Canvas;

    public Bounds Area => MainArea;

    public bool HasBoundingBox => BoundingBox is not null;

    /// <summary>Whether the bounding box should be drawn.</summary>
    public bool IsBoxShown => IsBoundingBoxVisible && BoundingBox is not null;

    /// <summary>The bounding box, or an empty one when there is none (for binding).</summary>
    public Bounds Box => BoundingBox ?? default;

    /// <summary>Corners of the visible bounding box: the extreme points.</summary>
    public IReadOnlyList<Point2D> ExtremeCorners => IsBoxShown ? Box.Corners.ToList() : [];

    public int OutlierCount => Shapes.Count(shape => shape.IsOutlier);

    public bool HasSelectedColors => Colors.Any(color => color.IsSelected);

    public string SummaryText =>
        $"{Shapes.Count} rectangles · {OutlierCount} outside the main area · {IncludedCount} used for the bounding box";

    public string BoundingBoxText => (IsBoundingBoxVisible, BoundingBox) switch
    {
        (false, _) => "Press “Highlight extreme points” to see the bounding box.",
        (true, null) => "No rectangle matches the current filters.",
        (true, { } box) => $"Left {box.Left:0} · Top {box.Top:0} · Right {box.Right:0} · Bottom {box.Bottom:0}\nSize {box.Width:0} × {box.Height:0}",
    };

    [RelayCommand]
    private void Generate()
    {
        foreach (var color in Colors)
        {
            color.PropertyChanged -= OnColorOptionChanged;
        }

        Shapes.Clear();
        Colors.Clear();

        foreach (var shape in _generator.Generate(ShapeCount, MainArea, Canvas))
        {
            Shapes.Add(new ShapeItemViewModel(shape, ExtremePointsCalculator.IsOutlier(shape, MainArea)));

            var option = new ColorOptionViewModel(shape.Color, shape.Number);
            option.PropertyChanged += OnColorOptionChanged;
            Colors.Add(option);
        }

        OnPropertyChanged(nameof(OutlierCount));
        OnPropertyChanged(nameof(HasSelectedColors));
        Recalculate();

        _log.Write($"Generated {Shapes.Count} rectangles ({OutlierCount} outliers).");
    }

    [RelayCommand]
    private void HighlightExtremePoints()
    {
        IsBoundingBoxVisible = true;
        Recalculate();
        _log.Write(BoundingBox is { } box
            ? $"Extreme points highlighted: {box} from {IncludedCount} rectangles."
            : "Extreme points requested, but no rectangle matches the filters.");
    }

    [RelayCommand]
    private void HideExtremePoints() => IsBoundingBoxVisible = false;

    [RelayCommand(CanExecute = nameof(HasSelectedColors))]
    private void ClearColorSelection()
    {
        foreach (var color in Colors)
        {
            color.IsSelected = false;
        }
    }

    partial void OnShapeCountChanged(int value)
    {
        var clamped = Math.Clamp(value, MinShapeCount, MaxShapeCount);
        if (clamped != value)
        {
            ShapeCount = clamped;
        }
    }

    partial void OnExcludeOutliersChanged(bool value) => Recalculate();

    partial void OnSelectedColorFilterChanged(ColorFilterOption value) => Recalculate();

    private void OnColorOptionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ColorOptionViewModel.IsSelected))
        {
            OnPropertyChanged(nameof(HasSelectedColors));
            ClearColorSelectionCommand.NotifyCanExecuteChanged();
            Recalculate();
        }
    }

    private void Recalculate()
    {
        var options = new ExtremePointsOptions(
            ExcludeOutliers,
            SelectedColorFilter?.Mode ?? ColorFilterMode.Off,
            Colors.Where(color => color.IsSelected).Select(color => color.Color).ToHashSet());

        var result = ExtremePointsCalculator.Calculate(Shapes.Select(shape => shape.Model), MainArea, options);
        var included = result.Included.Select(shape => shape.Number).ToHashSet();

        foreach (var shape in Shapes)
        {
            shape.IsIncluded = included.Contains(shape.Number);
        }

        BoundingBox = result.Box;
        IncludedCount = result.Included.Count;
        OnPropertyChanged(nameof(SummaryText));
    }
}

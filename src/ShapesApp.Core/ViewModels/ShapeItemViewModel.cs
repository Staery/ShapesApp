using CommunityToolkit.Mvvm.ComponentModel;
using ShapesApp.Core.Geometry;

namespace ShapesApp.Core.ViewModels;

/// <summary>A rectangle as drawn on the canvas.</summary>
public sealed partial class ShapeItemViewModel(RectangleShape model, bool isOutlier) : ObservableObject
{
    public RectangleShape Model { get; } = model;

    public int Number => Model.Number;

    public double X => Model.Bounds.X;

    public double Y => Model.Bounds.Y;

    public double Width => Model.Bounds.Width;

    public double Height => Model.Bounds.Height;

    public string Fill => Model.Color.Hex;

    /// <summary>Sticks out of the main area.</summary>
    public bool IsOutlier { get; } = isOutlier;

    /// <summary>Taken into account by the current extreme-points calculation.</summary>
    [ObservableProperty]
    private bool _isIncluded = true;

    public string Description =>
        $"Rectangle #{Number}  {Fill}\n{Model.Bounds}  ·  {Width:0}×{Height:0}" + (IsOutlier ? "\nOutside the main area" : string.Empty);
}

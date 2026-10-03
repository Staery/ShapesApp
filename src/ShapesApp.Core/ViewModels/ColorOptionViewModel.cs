using CommunityToolkit.Mvvm.ComponentModel;
using ShapesApp.Core.Geometry;
using ShapesApp.Core.Services;

namespace ShapesApp.Core.ViewModels;

/// <summary>A color that can be selected for the color filter.</summary>
public sealed partial class ColorOptionViewModel(ShapeColor color, int shapeNumber) : ObservableObject
{
    public ShapeColor Color { get; } = color;

    public string Hex => Color.Hex;

    public string Label => $"#{shapeNumber}  {Color.Hex}";

    [ObservableProperty]
    private bool _isSelected;
}

public sealed record ColorFilterOption(string Label, ColorFilterMode Mode);

namespace ShapesApp.Core.Geometry;

/// <summary>A colored rectangle placed on the canvas.</summary>
public sealed record RectangleShape(int Number, Bounds Bounds, ShapeColor Color);

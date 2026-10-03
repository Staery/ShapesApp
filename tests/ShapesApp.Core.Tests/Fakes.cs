using ShapesApp.Core.Geometry;
using ShapesApp.Core.Services;

namespace ShapesApp.Core.Tests;

internal sealed class MemoryLog : IActivityLog
{
    public List<string> Messages { get; } = [];

    public void Write(string message) => Messages.Add(message);
}

internal static class Shapes
{
    public static readonly Bounds Area = new(100, 100, 500, 250);

    public static RectangleShape Rect(int number, double x, double y, double width, double height, string color = "#FF0000") =>
        new(number, new Bounds(x, y, width, height), ShapeColor.Parse(color));
}

using ShapesApp.Core.Services;
using ShapesApp.Core.ViewModels;

namespace ShapesApp.Core.Tests;

public class MainViewModelTests
{
    private readonly MemoryLog _log = new();

    private MainViewModel Create(int seed = 3) => new(new ShapeGenerator(new Random(seed)), _log);

    [Fact]
    public void StartsWithGeneratedShapesAndHiddenBox()
    {
        var vm = Create();

        Assert.Equal(vm.ShapeCount, vm.Shapes.Count);
        Assert.Equal(vm.Shapes.Count, vm.Colors.Count);
        Assert.False(vm.IsBoundingBoxVisible);
        Assert.Single(_log.Messages);
    }

    [Fact]
    public void Generate_UsesShapeCountAndResetsColorSelection()
    {
        var vm = Create();
        vm.Colors[0].IsSelected = true;

        vm.ShapeCount = 12;
        vm.GenerateCommand.Execute(null);

        Assert.Equal(12, vm.Shapes.Count);
        Assert.False(vm.HasSelectedColors);
        Assert.False(vm.ClearColorSelectionCommand.CanExecute(null));
    }

    [Theory]
    [InlineData(0, MainViewModel.MinShapeCount)]
    [InlineData(1000, MainViewModel.MaxShapeCount)]
    public void ShapeCount_IsClamped(int requested, int expected)
    {
        var vm = Create();

        vm.ShapeCount = requested;

        Assert.Equal(expected, vm.ShapeCount);
    }

    [Fact]
    public void Highlight_ShowsBoxMatchingCalculator()
    {
        var vm = Create();

        vm.HighlightExtremePointsCommand.Execute(null);

        var expected = ExtremePointsCalculator.Calculate(
            vm.Shapes.Select(shape => shape.Model), MainViewModel.MainArea, ExtremePointsOptions.Default);
        Assert.True(vm.IsBoundingBoxVisible);
        Assert.Equal(expected.Box, vm.BoundingBox);
        Assert.Equal(expected.Included.Count, vm.IncludedCount);
        Assert.Equal(expected.Included.Count, vm.Shapes.Count(shape => shape.IsIncluded));
        Assert.Contains("Left", vm.BoundingBoxText);
    }

    [Fact]
    public void ChangingFilters_RecalculatesImmediately()
    {
        var vm = Create(seed: 11);
        vm.ShapeCount = 20;
        vm.GenerateCommand.Execute(null);
        vm.HighlightExtremePointsCommand.Execute(null);
        Assert.True(vm.OutlierCount > 0);

        vm.ExcludeOutliers = false;
        Assert.Equal(20, vm.IncludedCount);

        vm.SelectedColorFilter = vm.ColorFilters.Single(option => option.Mode == ColorFilterMode.OnlySelected);
        vm.Colors[0].IsSelected = true;
        Assert.Equal(1, vm.IncludedCount);
        Assert.Equal(vm.Shapes[0].Model.Bounds, vm.BoundingBox);

        vm.ClearColorSelectionCommand.Execute(null);
        Assert.Equal(20, vm.IncludedCount);
    }

    [Fact]
    public void MainShapeIsNeverRemovedByHighlighting()
    {
        // Regression: the old version tagged the bounding box like the main rectangle and deleted the main one on first click.
        var vm = Create();
        var before = vm.Shapes.ToList();

        vm.HighlightExtremePointsCommand.Execute(null);
        vm.HighlightExtremePointsCommand.Execute(null);

        Assert.Equal(before, vm.Shapes);
    }

    [Fact]
    public void NoMatchingShapes_ReportsEmptyResult()
    {
        var vm = Create();
        vm.HighlightExtremePointsCommand.Execute(null);

        vm.SelectedColorFilter = vm.ColorFilters.Single(option => option.Mode == ColorFilterMode.ExceptSelected);
        foreach (var color in vm.Colors)
        {
            color.IsSelected = true;
        }

        Assert.Null(vm.BoundingBox);
        Assert.False(vm.HasBoundingBox);
        Assert.Equal(0, vm.IncludedCount);
        Assert.Contains("No rectangle", vm.BoundingBoxText);
    }

    [Fact]
    public void FileLog_AppendsTimestampedLines()
    {
        var path = Path.Combine(Path.GetTempPath(), "shapesapp-" + Guid.NewGuid().ToString("N"), "activity.log");
        try
        {
            var log = new FileActivityLog(path, TimeProvider.System);

            log.Write("first");
            log.Write("second");

            var lines = File.ReadAllLines(path);
            Assert.Equal(2, lines.Length);
            Assert.EndsWith("second", lines[1]);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }
}

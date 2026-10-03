using System.Windows;
using System.Windows.Threading;
using ShapesApp.Core.Services;
using ShapesApp.Core.ViewModels;

namespace ShapesApp;

/// <summary>Composition root: wires services and view models together and shows the main window.</summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        var viewModel = new MainViewModel(new ShapeGenerator(), FileActivityLog.CreateDefault());

        MainWindow = new MainWindow { DataContext = viewModel };
        MainWindow.Show();
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Something went wrong:\n\n{e.Exception.Message}", "ShapesApp", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}

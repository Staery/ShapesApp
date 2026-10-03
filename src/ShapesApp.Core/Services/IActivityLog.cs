namespace ShapesApp.Core.Services;

/// <summary>Records what the user did, for troubleshooting.</summary>
public interface IActivityLog
{
    void Write(string message);
}

/// <summary>Appends timestamped lines to a text file. Logging never interrupts the app.</summary>
public sealed class FileActivityLog(string filePath, TimeProvider time) : IActivityLog
{
    public string FilePath { get; } = filePath;

    public static FileActivityLog CreateDefault() => new(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ShapesApp", "activity.log"),
        TimeProvider.System);

    public void Write(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.AppendAllText(FilePath, $"{time.GetLocalNow():yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A broken log must not break the app.
        }
    }
}

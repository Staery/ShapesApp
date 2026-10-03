using System.Globalization;

namespace ShapesApp.Core.Geometry;

/// <summary>An opaque RGB color, independent of any UI framework.</summary>
public readonly record struct ShapeColor(byte R, byte G, byte B)
{
    public string Hex => $"#{R:X2}{G:X2}{B:X2}";

    public static ShapeColor Parse(string hex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hex);

        var digits = hex.TrimStart('#');
        if (digits.Length != 6 || !int.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"'{hex}' is not a #RRGGBB color.");
        }

        return new ShapeColor((byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    /// <summary>Converts HSV (hue in degrees, saturation and value in 0..1) to RGB.</summary>
    public static ShapeColor FromHsv(double hue, double saturation, double value)
    {
        hue = ((hue % 360) + 360) % 360;
        saturation = Math.Clamp(saturation, 0, 1);
        value = Math.Clamp(value, 0, 1);

        var chroma = value * saturation;
        var x = chroma * (1 - Math.Abs((hue / 60) % 2 - 1));
        var m = value - chroma;

        var (r, g, b) = (int)(hue / 60) switch
        {
            0 => (chroma, x, 0d),
            1 => (x, chroma, 0d),
            2 => (0d, chroma, x),
            3 => (0d, x, chroma),
            4 => (x, 0d, chroma),
            _ => (chroma, 0d, x),
        };

        return new ShapeColor(ToByte(r + m), ToByte(g + m), ToByte(b + m));

        static byte ToByte(double channel) => (byte)Math.Round(channel * 255);
    }

    public override string ToString() => Hex;
}

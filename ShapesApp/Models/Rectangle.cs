using System.Windows.Media;

namespace ShapesApp.Models
{
    public class Rectangle
    {
        public Color Color { get; set; }
        public Point BottomLeft { get; set; }
        public Point BottomRight { get; set; }
        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public string Tag { get; set; }
    }
}

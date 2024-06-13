using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using ShapesApp.Commands;
using ShapesApp.Models;

namespace ShapesApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Random random = new Random();
        private Rectangle MainRectangle { get; set; }
        private bool excludeOutliers = true;
        public ObservableCollection<Rectangle> Rectangles { get; set; }
        public ObservableCollection<Color> UsedColors { get; set; }
        public ObservableCollection<Color> SelectedColors { get; set; }
        public ICommand HighlightExtremePointsCommand { get; }
        public ICommand GenerateShapesCommand { get; }
        public bool IncludeColorFilter { get; set; }

        public bool ExcludeOutliers
        {
            get => excludeOutliers;
            set
            {
                if (excludeOutliers != value)
                {
                    excludeOutliers = value;
                    OnPropertyChanged(nameof(ExcludeOutliers));
                }
            }
        }

        public MainViewModel()
        {
            Rectangles = new ObservableCollection<Rectangle>();
            UsedColors = new ObservableCollection<Color>();
            SelectedColors = new ObservableCollection<Color>();
            HighlightExtremePointsCommand = new RelayCommand<object>(HighlightExtremePoints);
            GenerateShapesCommand = new RelayCommand(GenerateShapes);
            GenerateShapes();
        }

        private void GenerateShapes()
        {
            UsedColors.Clear();
            Rectangles.Clear();
            MainRectangle = new Rectangle
            {
                Color = Colors.White,
                BottomLeft = new Point(100, 100),
                BottomRight = new Point(600, 100),
                TopLeft = new Point(100, 300),
                TopRight = new Point(600, 300),
                Tag = "Main"
            };
            Rectangles.Add(MainRectangle);

            for (int i = 0; i < 5; i++)
            {
                Rectangle randomRectangle = GenerateRandomRectangle();
                Rectangles.Add(randomRectangle);
                if (!UsedColors.Contains(randomRectangle.Color))
                {
                    UsedColors.Add(randomRectangle.Color);
                }
            }
        }

        private Rectangle GenerateRandomRectangle()
        {
            Point randomBottomLeft = new Point(random.Next(100, 600), random.Next(100, 300));
            double width = random.Next(20, 150);
            double height = random.Next(20, 150);
            Point randomBottomRight = new Point(randomBottomLeft.X + width, randomBottomLeft.Y);
            Point randomTopLeft = new Point(randomBottomLeft.X, randomBottomLeft.Y + height);
            Point randomTopRight = new Point(randomBottomRight.X, randomTopLeft.Y);

            Color randomColor = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

            return new Rectangle
            {
                Color = randomColor,
                BottomLeft = randomBottomLeft,
                BottomRight = randomBottomRight,
                TopLeft = randomTopLeft,
                TopRight = randomTopRight
            };
        }

        private void HighlightExtremePoints(object parameter)
        {

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            SelectedColors = new ObservableCollection<Color>();

            IList colorFilterListBox = (IList)parameter;

            foreach (object selectedItem in colorFilterListBox) 
            {
                string colorTag = selectedItem.ToString();
                Color color = (Color)ColorConverter.ConvertFromString(colorTag);
                SelectedColors.Add(color);
            }

            var filterColors = SelectedColors;

            RemoveOldRectangle();

            foreach (var rectangle in Rectangles)
            {
                if ((!ExcludeOutliers || IsPointInsideMainRectangle(rectangle)) &&
                    (filterColors.Count == 0 || (IncludeColorFilter && filterColors.Contains(rectangle.Color)) || (!IncludeColorFilter && !filterColors.Contains(rectangle.Color))))
                {
                    var points = new[]
                    {
                        rectangle.BottomLeft,
                        rectangle.BottomRight,
                        rectangle.TopLeft,
                        rectangle.TopRight,
                    };

                    foreach (var point in points)
                    {
                        minX = Math.Min(minX, point.X);
                        minY = Math.Min(minY, point.Y);
                        maxX = Math.Max(maxX, point.X);
                        maxY = Math.Max(maxY, point.Y);
                    }
                }
            }

            var extremeRectangle = new Rectangle
            {
                BottomLeft = new Point(minX, minY),
                BottomRight = new Point(maxX, minY),
                TopLeft = new Point(minX, maxY),
                TopRight = new Point(maxX, maxY),
                Color = Colors.Transparent, // or any other color that you use to highlight the extreme rectangle
                Tag = "Main"
            };

            Rectangles.Add(extremeRectangle);
        }

        private void RemoveOldRectangle()
        {
            foreach (var rectangle in Rectangles)
            {
                if (rectangle != null && rectangle.Tag == "Main") { Rectangles.Remove(rectangle); break; }
            }
        }
    

    private bool IsPointInsideMainRectangle(Rectangle rectangle)
        {
            foreach (var point in new[] { rectangle.BottomLeft, rectangle.BottomRight, rectangle.TopLeft, rectangle.TopRight })
            {
                if (point.X < MainRectangle.BottomLeft.X || point.X > MainRectangle.BottomRight.X ||
                    point.Y < MainRectangle.BottomLeft.Y || point.Y > MainRectangle.TopLeft.Y)
                {
                    return false;
                }
            }
            return true;
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

   
}

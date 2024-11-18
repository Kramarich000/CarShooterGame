using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GameApp.Models
{
    public class Coin
    {
        public FrameworkElement Element { get; set; }
        public Point Coords { get; set; }
        public double Height { get; private set; }
        public double Width { get; private set; }
        public bool Visible { get; private set; }

        private static readonly Random random = new Random();
        private readonly double roadLeft = 510;
        private readonly double roadRight = 1370;
        private readonly List<FrameworkElement> allObjects;

        private readonly Dictionary<int, HashSet<FrameworkElement>> spatialGrid;
        private const int GridCellSize = 200;

        public Coin(FrameworkElement element, List<FrameworkElement> allObjects)
        {
            Element = element;
            this.allObjects = allObjects;
            spatialGrid = BuildSpatialGrid(allObjects);

            Element.Loaded += (sender, e) =>
            {
                Height = element.ActualHeight;
                Width = element.ActualWidth;
                Reset();
            };
            Coords = new Point(0, 0);
            Visible = true;
        }

        public void Move(double speed)
        {
            Coords = new Point(Coords.X, Coords.Y + speed);

            if (Coords.Y > Application.Current.MainWindow.Height)
            {
                Reset();
            }
            else
            {
                MoveElement();
            }
        }

        private bool IsTooCloseToOtherElements(Point potentialCoords, double minDistance)
        {
            foreach (var obj in GetNearbyObjects(potentialCoords))
            {
                if (ReferenceEquals(obj, Element)) continue;

                double otherX = Canvas.GetLeft(obj) + obj.ActualWidth / 2;
                double otherY = Canvas.GetTop(obj) + obj.ActualHeight / 2;

                double dx = potentialCoords.X + Width / 2 - otherX;
                double dy = potentialCoords.Y + Height / 2 - otherY;

                if (dx * dx + dy * dy < minDistance * minDistance)
                {
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            int maxAttempts = 10;
            int attempts = 0;
            int minDistance = 100;

            Element.Visibility = Visibility.Hidden;

            do
            {
                attempts++;
                var randomYCoord = random.Next(-1000, -(int)Height);
                int randomXCoord = random.Next((int)roadLeft, (int)(roadRight - Width));

                var potentialCoords = new Point(randomXCoord, randomYCoord);

                if (!IsTooCloseToOtherElements(potentialCoords, minDistance))
                {
                    Coords = potentialCoords;
                    Element.Visibility = Visibility.Visible;
                    Visible = true;
                    MoveElement();
                    return;
                }

            } while (attempts < maxAttempts);

            Console.WriteLine("Не удалось разместить элемент после нескольких попыток.");
        }

        public void MoveElement()
        {
            Canvas.SetLeft(Element, Coords.X);
            Canvas.SetTop(Element, Coords.Y);
        }

        private Dictionary<int, HashSet<FrameworkElement>> BuildSpatialGrid(List<FrameworkElement> objects)
        {
            var grid = new Dictionary<int, HashSet<FrameworkElement>>();

            foreach (var obj in objects)
            {
                int cellIndex = GetGridCellIndex(Canvas.GetLeft(obj), Canvas.GetTop(obj));
                if (!grid.ContainsKey(cellIndex))
                    grid[cellIndex] = new HashSet<FrameworkElement>();

                grid[cellIndex].Add(obj);
            }

            return grid;
        }

        private IEnumerable<FrameworkElement> GetNearbyObjects(Point coords)
        {
            int cellIndex = GetGridCellIndex(coords.X, coords.Y);

            if (spatialGrid.TryGetValue(cellIndex, out var objects))
                return objects;

            return Array.Empty<FrameworkElement>();
        }

        private int GetGridCellIndex(double x, double y)
        {
            int cellX = (int)(x / GridCellSize);
            int cellY = (int)(y / GridCellSize);
            return cellX + cellY * 1000; 
        }
    }
}

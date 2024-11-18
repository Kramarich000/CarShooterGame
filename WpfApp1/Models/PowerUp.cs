using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GameApp.Models
{
    public class PowerUp
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

        private const int GridCellSize = 200;
        private readonly Dictionary<int, HashSet<FrameworkElement>> spatialGrid;

        public PowerUp(FrameworkElement element, List<FrameworkElement> allObjects)
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

        public void Reset()
        {
            int maxAttempts = 10;
            int attempts = 0;
            int minDistance = 100;
            double minDistanceSquared = minDistance * minDistance;

            Element.Visibility = Visibility.Hidden;

            do
            {
                attempts++;
                var randomYCoord = random.Next(-1500, -(int)Height - 1000);
                int randomXCoord = random.Next((int)roadLeft, (int)(roadRight - Width));
                Coords = new Point(randomXCoord, randomYCoord);

                bool collisionDetected = false;

                foreach (var obj in GetNearbyObjects(randomXCoord, randomYCoord))
                {
                    if (ReferenceEquals(obj, Element)) continue;

                    if (CheckCollision(obj) || CalculateDistanceSquared(obj) < minDistanceSquared)
                    {
                        collisionDetected = true;
                        break;
                    }
                }

                if (!collisionDetected)
                {
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

        private double CalculateDistanceSquared(FrameworkElement otherElement)
        {
            double obstacleCenterX = Coords.X + Width / 2;
            double obstacleCenterY = Coords.Y + Height / 2;

            double otherCenterX = Canvas.GetLeft(otherElement) + otherElement.ActualWidth / 2;
            double otherCenterY = Canvas.GetTop(otherElement) + otherElement.ActualHeight / 2;

            double dx = obstacleCenterX - otherCenterX;
            double dy = obstacleCenterY - otherCenterY;

            return dx * dx + dy * dy;
        }

        private bool CheckCollision(FrameworkElement otherElement)
        {
            var coinRect = new Rect(Canvas.GetLeft(Element), Canvas.GetTop(Element), Width, Height);
            var otherRect = new Rect(Canvas.GetLeft(otherElement), Canvas.GetTop(otherElement), otherElement.ActualWidth, otherElement.ActualHeight);

            return coinRect.IntersectsWith(otherRect);
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

        private IEnumerable<FrameworkElement> GetNearbyObjects(double x, double y)
        {
            int cellIndex = GetGridCellIndex(x, y);

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

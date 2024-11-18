using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System;

public class Obstacle
{
    public FrameworkElement Element { get; }
    public Point Coords => new Point(Canvas.GetLeft(Element), Canvas.GetTop(Element));
    public double Width => Element.ActualWidth;
    public double Height => Element.ActualHeight;

    private double speed;
    private readonly double roadLeft = 560;
    private readonly double roadRight = 1340;
    private static readonly Random random = new Random();
    private readonly List<FrameworkElement> allObjects;

    private readonly Dictionary<int, HashSet<FrameworkElement>> spatialGrid;
    private const int GridCellSize = 200; 

    public Obstacle(FrameworkElement element, double speed, List<FrameworkElement> allObjects)
    {
        Element = element;
        this.speed = speed;
        this.allObjects = allObjects;
        spatialGrid = BuildSpatialGrid(allObjects);
        Reset();
    }

    public void Move(double speed)
    {
        Canvas.SetTop(Element, Canvas.GetTop(Element) + speed);

        if (Canvas.GetTop(Element) > Application.Current.MainWindow.Height)
        {
            Reset();
        }
    }

    public void Reset()
    {
        bool collisionDetected;
        int maxAttempts = 5;
        int attempts = 0;
        int minDistance = 100;

        Element.Visibility = Visibility.Hidden;

        do
        {
            collisionDetected = false;
            attempts++;

            var randomYCoord = random.Next(-1000, -(int)Height);
            var randomXCoord = random.Next((int)roadLeft, (int)(roadRight - Width));

            Canvas.SetLeft(Element, randomXCoord);
            Canvas.SetTop(Element, randomYCoord);

            foreach (var obj in GetNearbyObjects(randomXCoord, randomYCoord))
            {
                if (ReferenceEquals(obj, Element)) continue;

                if (CheckCollision(obj) || IsTooClose(obj, minDistance))
                {
                    collisionDetected = true;
                    break;
                }
            }

            if (attempts >= maxAttempts)
            {
                Console.WriteLine("Не удалось разместить элемент после нескольких попыток.");
                break;
            }

        } while (collisionDetected);

        Element.Visibility = Visibility.Visible;
    }

    private bool CheckCollision(FrameworkElement otherElement)
    {
        var obstacleRect = new Rect(Canvas.GetLeft(Element), Canvas.GetTop(Element), Width, Height);
        var otherRect = new Rect(Canvas.GetLeft(otherElement), Canvas.GetTop(otherElement), otherElement.ActualWidth, otherElement.ActualHeight);

        return obstacleRect.IntersectsWith(otherRect);
    }

    private bool IsTooClose(FrameworkElement otherElement, double minDistance)
    {
        double obstacleCenterX = Coords.X + Width / 2;
        double obstacleCenterY = Coords.Y + Height / 2;

        double otherCenterX = Canvas.GetLeft(otherElement) + otherElement.ActualWidth / 2;
        double otherCenterY = Canvas.GetTop(otherElement) + otherElement.ActualHeight / 2;

        double dx = obstacleCenterX - otherCenterX;
        double dy = obstacleCenterY - otherCenterY;

        return dx * dx + dy * dy < minDistance * minDistance;
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

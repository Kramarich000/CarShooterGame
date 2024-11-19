using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

public class Tree
{
    public UIElement Element { get; set; }
    public Point Coords { get; set; }
    public double Height { get; set; }
    public double Width { get; set; }
    private readonly Random random;
    private readonly double minDistance = 150;

    public Tree(UIElement element, Random random, List<Tree> trees)
    {
        this.random = random;
        Element = element;

        if (element is FrameworkElement frameworkElement)
        {
            frameworkElement.Loaded += (sender, e) =>
            {
                Height = frameworkElement.RenderSize.Height;
                Width = frameworkElement.RenderSize.Width;
            };
        }

        Coords = new Point(Canvas.GetLeft(element), Canvas.GetTop(element));
        Element.Visibility = Visibility.Visible;

        if (Coords.X == 0 && Coords.Y == 0)
        {
            PositionTreeOutsideRoad(trees);
        }
    }

    public void PositionTreeOutsideRoad(List<Tree> trees)
    {
        double roadLeft = 410;
        double roadRight = 1530;
        double x;

        if (random.NextDouble() < 0.5)
        {
            x = random.NextDouble() * roadLeft;
        }
        else
        {
            x = roadRight + random.NextDouble() * (Application.Current.MainWindow.ActualWidth - roadRight);
        }

        double y = -random.NextDouble() * Application.Current.MainWindow.ActualHeight - 400;

        Coords = new Point(x, y);

        while (IsTooCloseToOtherTrees(trees))
        {
            if (random.NextDouble() < 0.5)
            {
                x = random.NextDouble() * roadLeft;
            }
            else
            {
                x = roadRight + random.NextDouble() * (Application.Current.MainWindow.ActualWidth - roadRight);
            }

            y = -random.NextDouble() * Application.Current.MainWindow.ActualHeight - 400;

            Coords = new Point(x, y);
        }
    }


    public void Move(double speed, List<Tree> trees)
    {
        var canvas = Application.Current.MainWindow.FindName("GameCanvas") as Canvas;

        Coords = new Point(Coords.X, Coords.Y + speed);

        if (Coords.Y > canvas.ActualHeight)
        {
            PositionTreeOutsideRoad(trees);
        }

        List<UIElement> uiElements = trees.ConvertAll(tree => tree.Element);

        if (CheckCollisionWithOtherObjects(uiElements))
        {
            PositionTreeOutsideRoad(trees);
        }

        if (IsTooCloseToOtherTrees(trees))
        {
            PositionTreeOutsideRoad(trees);
        }

        MoveElement();
    }


    private bool CheckCollisionWithOtherObjects(List<UIElement> allObjects)
    {
        foreach (var obj in allObjects)
        {
            if (obj != this.Element && IsCollision(obj))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsCollision(UIElement otherElement)
    {
        var treeRect = new Rect(Canvas.GetLeft(Element), Canvas.GetTop(Element), Width, Height);
        var otherRect = new Rect(Canvas.GetLeft(otherElement), Canvas.GetTop(otherElement), otherElement.RenderSize.Width, otherElement.RenderSize.Height);
        return treeRect.IntersectsWith(otherRect);
    }

    public void MoveElement()
    {
        Canvas.SetLeft(Element, Coords.X);
        Canvas.SetTop(Element, Coords.Y);
        Element.Visibility = Visibility.Visible;
    }

    private bool IsTooCloseToOtherTrees(List<Tree> trees)
    {
        foreach (var tree in trees)
        {
            if (tree != this && CalculateDistanceToTree(tree) < minDistance)
            {
                return true;
            }
        }
        return false;
    }

    private double CalculateDistanceToTree(Tree otherTree)
    {
        double dx = Coords.X - otherTree.Coords.X;
        double dy = Coords.Y - otherTree.Coords.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }



}

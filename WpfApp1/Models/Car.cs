using System;
using System.Windows;
using System.Windows.Controls;

public class Car
{
    public FrameworkElement Element { get; set; }
    public Point Coords { get; set; }
    public double Height => Element.ActualHeight;
    public double Width => Element.ActualWidth;
    public bool IsMovingTop { get; set; }
    public bool IsMovingBottom { get; set; }
    public bool IsMovingLeft { get; set; }
    public bool IsMovingRight { get; set; }

    private readonly double roadLeft = 510;
    private readonly double roadRight = 1430;
    private readonly double roadTop = 0;
    public readonly Visibility Visibility = Visibility.Visible;

    private Point initialPosition;

    public Car(FrameworkElement element, Canvas canvas)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        Element = element;
        Coords = new Point(Canvas.GetLeft(element), Canvas.GetTop(element));
        initialPosition = Coords;
    }

    public void MoveToTop(Canvas canvas)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (IsMovingTop)
        {
            Coords = new Point(Coords.X, Coords.Y - 5);
            if (Coords.Y < roadTop) Coords = new Point(Coords.X, roadTop);
            MoveElement();
        }
    }

    public void MoveToBottom(Canvas canvas)
    {
        if (IsMovingBottom)
        {
            if (Coords.Y + Height + 4 <= canvas.Height)
            {
                Coords = new Point(Coords.X, Coords.Y + 4);
            }
            else
            {
                Coords = new Point(Coords.X, canvas.Height - Height);
            }

            MoveElement();
        }
    }

    public void MoveToLeft(Canvas canvas, double LeftSpeed)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (IsMovingLeft)
        {
            Coords = new Point(Coords.X - LeftSpeed, Coords.Y);
            if (Coords.X < roadLeft) Coords = new Point(roadLeft, Coords.Y);
            MoveElement();
        }
    }

    public void MoveToRight(Canvas canvas, double RightSpeed)
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (IsMovingRight)
        {
            Coords = new Point(Coords.X + RightSpeed, Coords.Y);
            if (Coords.X + Width > roadRight) Coords = new Point(roadRight - Width, Coords.Y);
            MoveElement();
        }
    }

    public void MoveElement()
    {
        Canvas.SetLeft(Element, Coords.X);
        Canvas.SetTop(Element, Coords.Y);
    }

    public bool HasCollision(FrameworkElement other)
    {
        var otherCoords = new Point(Canvas.GetLeft(other), Canvas.GetTop(other));
        var otherHeight = other.ActualHeight;
        var otherWidth = other.ActualWidth;

        Rect thisRect = new Rect(Coords.X, Coords.Y, Width, Height);
        Rect otherRect = new Rect(otherCoords.X, otherCoords.Y, otherWidth, otherHeight);

        if (thisRect.IntersectsWith(otherRect))
        {
            return true;
        }

        return false;
    }
    public void ResetToInitialPosition()
    {
        Coords = initialPosition;
        MoveElement();
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public class Bullet
{
    public UIElement Element { get; set; }
    public Point Coords { get; set; }
    public bool Visible { get; set; }
    private DateTime lastFireTime = DateTime.MinValue;
    private TimeSpan fireCooldown = TimeSpan.FromMilliseconds(500);

    public Bullet(UIElement element)
    {
        Element = element;
        Coords = new Point(0, 0);
        Visible = false;
    }

    public void Fire(double x, double y)
    {
        if (Visible)
            return;

        var currentTime = DateTime.Now;
        if (currentTime - lastFireTime < fireCooldown)
            return;

        Coords = new Point(x, y);

        Visible = true;

        lastFireTime = currentTime;
    }




    public void Move(double speed)
    {
        if (Visible)
        {
            double movementSpeed = 10;
            Coords = new Point(Coords.X, Coords.Y - movementSpeed);

            if (Coords.Y < 0)
            {
                Hide();
            }
            else
            {
                MoveElement();
            }
        }
    }



    public void Hide()
    {
        Visible = false;
        Element.Visibility = Visibility.Hidden;
    }

    public void MoveElement()
    {
        Element.RenderTransform = new TranslateTransform(Coords.X, Coords.Y);
    }

    public bool HasCollision(UIElement other)
    {
        var bulletRect = new Rect(Coords.X, Coords.Y, Element.RenderSize.Width, Element.RenderSize.Height);

        var enemyX = Canvas.GetLeft(other);
        var enemyY = Canvas.GetTop(other);
        var enemyWidth = other.RenderSize.Width;
        var enemyHeight = other.RenderSize.Height;

        var enemyRect = new Rect(enemyX, enemyY, enemyWidth, enemyHeight);

        if (bulletRect.IntersectsWith(enemyRect))
        {
            return true;
        }

        return false;
    }

}
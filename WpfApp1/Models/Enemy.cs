using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GameApp.Models
{
    public class Enemy
    {
        public FrameworkElement Element { get; set; }
        public Point Coords { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Speed { get; set; }
        public bool Visible { get; set; }

        private static readonly Random random = new Random();

        private readonly double roadLeft = 510;
        private readonly double roadRight = 1370;

        public Enemy(FrameworkElement element, double speed)
        {
            Element = element;
            Coords = new Point(0, 0);
            Height = element.ActualHeight;
            Width = element.ActualWidth;
            Speed = speed;
            Visible = true;
        }

        public void Move(double speed, List<Enemy> enemiesList)
        {
            double newY = Coords.Y + speed;

            if (newY > Application.Current.MainWindow.Height)
            {
                Coords = new Point(Coords.X, Application.Current.MainWindow.Height);
                Reset(enemiesList);
            }
            else
            {
                Coords = new Point(Coords.X, newY);
                MoveElement();
            }
        }


        public void Reset(List<Enemy> enemies)
        {
            int MaxAttempts = 5;
            int MinDistanceBetweenEnemies = 50;
            var randomXCoord = random.Next((int)roadLeft, (int)roadRight - (int)Width);
            var randomYCoord = random.Next(-1000, -((int)Height) - 1000);

            bool intersects = true;
            int attempts = 0;

            while (intersects && attempts < MaxAttempts)
            {
                intersects = false;
                attempts++;

                foreach (var enemy in enemies)
                {
                    if (enemy != this)
                    {
                        double distance = Math.Sqrt(Math.Pow(randomXCoord - enemy.Coords.X, 2) +
                                                    Math.Pow(randomYCoord - enemy.Coords.Y, 2));

                        if (distance < MinDistanceBetweenEnemies || IsIntersecting(randomXCoord, randomYCoord, enemy))
                        {
                            intersects = true;
                            randomXCoord = random.Next((int)roadLeft, (int)roadRight - (int)Width);
                            randomYCoord = random.Next(-1000, -((int)Height) - 500);
                            break;
                        }
                    }
                }
            }

            if (attempts >= MaxAttempts)
            {
                randomXCoord = random.Next((int)roadLeft, (int)roadRight - (int)Width);
                randomYCoord = random.Next(-1000, -((int)Height) - 500);
            }

            Coords = new Point(randomXCoord, randomYCoord);
            Element.Visibility = Visibility.Visible;
            Visible = true;
            MoveElement();
        }


        private bool IsIntersecting(double x, double y, Enemy otherEnemy)
        {
            var enemyRect = new Rect(x, y, Width, Height);
            var otherRect = new Rect(otherEnemy.Coords.X, otherEnemy.Coords.Y, otherEnemy.Width, otherEnemy.Height);

            bool isIntersecting = enemyRect.IntersectsWith(otherRect);

            if (!isIntersecting)
            {
                double minDistance = 200;
                double centerX = x + Width / 2;
                double centerY = y + Height / 2;
                double otherCenterX = otherEnemy.Coords.X + otherEnemy.Width / 2;
                double otherCenterY = otherEnemy.Coords.Y + otherEnemy.Height / 2;

                double distance = Math.Sqrt(Math.Pow(centerX - otherCenterX, 2) + Math.Pow(centerY - otherCenterY, 2));

                isIntersecting = distance < minDistance;
            }

            return isIntersecting;
        }


        public void MoveElement()
        {
            Canvas.SetLeft(Element, Coords.X);
            Canvas.SetTop(Element, Coords.Y);
        }

        public bool HasCollision(FrameworkElement otherElement)
        {
            var enemyRect = new Rect(Canvas.GetLeft(Element), Canvas.GetTop(Element), Element.Width, Element.Height);
            var obstacleRect = new Rect(Canvas.GetLeft(otherElement), Canvas.GetTop(otherElement), otherElement.Width, otherElement.Height);

            return enemyRect.IntersectsWith(obstacleRect);
        }

    }
}

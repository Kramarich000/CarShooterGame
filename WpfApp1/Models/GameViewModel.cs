using GameApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using GameApp;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;

public class GameViewModel : INotifyPropertyChanged
{
    private Car car;
    private List<Enemy> enemies;
    private List<Coin> coins;
    private List<Tree> trees;
    private List<PowerUp> powerUps;
    private bool isPause;
    private int score;
    private double speed;
    private double enemySpeed;
    private readonly DispatcherTimer gameTimer;
    private readonly string difficulty;
    private List<Obstacle> obstacles;
    private readonly List<Bullet> bullets;
    private double turnSpeed = 4;
    private bool isPowerUpActive = false;
    private readonly List<DispatcherTimer> activeTimers = new List<DispatcherTimer>();
    public bool isGameRunning = true;
    private bool BruteStop = false;


    public event PropertyChangedEventHandler PropertyChanged;

    public Car Car
    {
        get => car;
        set
        {
            car = value;
            OnPropertyChanged();
        }
    }

    public List<Enemy> Enemies
    {
        get => enemies;
        set
        {
            enemies = value;
            OnPropertyChanged();
        }
    }

    public List<Obstacle> Obstacles
    {
        get => obstacles;
        set
        {
            obstacles = value;
            OnPropertyChanged();
        }
    }

    public List<Coin> Coins
    {
        get => coins;
        set
        {
            coins = value;
            OnPropertyChanged();
        }
    }

    public List<PowerUp> PowerUps
    {
        get => powerUps;
        set
        {
            powerUps = value;
            OnPropertyChanged();
        }
    }

    public List<Tree> Trees
    {
        get => trees;
        set
        {
            trees = value;
            OnPropertyChanged();
        }
    }

    public bool IsPause
    {
        get => isPause;
        set
        {
            isPause = value;
            OnPropertyChanged();
        }
    }

    public int Score
    {
        get => score;
        set
        {
            score = value;
            OnPropertyChanged();
        }
    }

    public double Speed
    {
        get => speed;
        set
        {
            speed = value;
            OnPropertyChanged();
        }
    }

    public double EnemySpeed
    {
        get => enemySpeed;
        set
        {
            enemySpeed = value;
            OnPropertyChanged();
        }
    }

    public GameViewModel(string difficulty)
    {
        this.difficulty = difficulty;
        IsPause = false;
        Score = 0;
        Speed = 2;
        EnemySpeed = GetEnemySpeed(difficulty);
        bullets = new List<Bullet>();
        enemies = new List<Enemy>();
        obstacles = new List<Obstacle>();
        coins = new List<Coin>();
        powerUps = new List<PowerUp>();

        InitializeGameObjects();

    }

    private double GetEnemySpeed(string difficulty)
    {
        switch (difficulty)
        {
            case "easy":
                return 3;
            case "medium":
                return 5;
            case "hard":
                return 6;
            default:
                throw new System.ArgumentException("Invalid difficulty");
        }
    }

    private void InitializeGameObjects()
    {
        var mainWindow = Application.Current.MainWindow;
        var canvas = mainWindow.FindName("GameCanvas") as Canvas;

        Car = new Car((FrameworkElement)(canvas.FindName("Car") as UIElement), canvas);

        Trees = new List<Tree>();
        Random random = new Random();

        List<FrameworkElement> allObjects = new List<FrameworkElement>();

        for (int i = 0; i < random.Next(2, 6); i++)
        {
            var obstacleElement = new Rectangle
            {
                Width = 100,
                Height = 20,
                Fill = Brushes.Red,
                Visibility = Visibility.Visible,
                Tag = "dynamic"
            };
            var obstacle = new Obstacle(obstacleElement, Speed, allObjects);
            obstacles.Add(obstacle);
            allObjects.Add(obstacleElement);
            canvas.Children.Add(obstacleElement);
        }

        for (int i = 0; i < random.Next(3, 8); i++)
        {
            var coinElement = new Image
            {
                Width = 30,
                Height = 30,
                Source = new BitmapImage(new Uri("pack://application:,,,/images/coin.png")),
                Visibility = Visibility.Visible,
                Tag = "dynamic"
            };
            var coin = new Coin(coinElement, allObjects);
            coins.Add(coin);
            canvas.Children.Add(coinElement);
        }

        for (int i = 0; i < 1; i++)
        {
            var PowerUpElement = new Image
            {
                Width = 30,
                Height = 30,
                Source = new BitmapImage(new Uri("pack://application:,,,/images/power.png")),
                Visibility = Visibility.Visible,
                Tag = "dynamic"
            };
            var powerup = new PowerUp(PowerUpElement, allObjects);
            powerUps.Add(powerup);
            canvas.Children.Add(PowerUpElement);
        }

        for (int i = 0; i < random.Next(3, 8); i++)
        {
            var enemyElement = new Image
            {
                Width = 50,
                Height = 100,
                Source = new BitmapImage(new Uri("pack://application:,,,/images/EnCar.png")),
                Visibility = Visibility.Visible,
                Tag = "dynamic"
            };

            var enemy = new Enemy(enemyElement, EnemySpeed);
            enemy.Reset(enemies);
            enemies.Add(enemy);
            allObjects.Add(enemyElement);
            canvas.Children.Add(enemyElement);
        }

        foreach (var tree in canvas.Children)
        {
            if (tree is FrameworkElement treeElement && treeElement.Name.StartsWith("Tree"))
            {
                Tree newTree = new Tree(treeElement, random, trees);
                Trees.Add(newTree);
                allObjects.Add(treeElement);
            }
        }

        foreach (var bullet in bullets)
        {
            canvas.Children.Add(bullet.Element);
        }
        RemoveOffScreenBullets();
    }

    public async void FireBullet(double x, double y)
    {
        int MaxBulletsOnScreen = 5;

        if (bullets.Count >= MaxBulletsOnScreen)
        {
            return;
        }

        var bulletElement = new Image
        {
            Width = 10,
            Height = 20,
            Source = new BitmapImage(new Uri("pack://application:,,,/images/bullet.png")),
            Visibility = Visibility.Hidden,
            Tag = "dynamic"
        };

        var bullet = new Bullet(bulletElement);
        bullet.Fire(x, y);

        bullets.Add(bullet);

        var mainWindow = Application.Current.MainWindow;
        var canvas = mainWindow.FindName("GameCanvas") as Canvas;
        canvas.Children.Add(bullet.Element);

        await Task.Delay(10); 

        bulletElement.Visibility = Visibility.Visible;
    }
    public void RemoveOffScreenBullets()
    {
        var bulletsToRemove = new List<Bullet>();

        foreach (var bullet in bullets)
        {
            if (!bullet.Visible || bullet.Coords.Y < -10)
            {
                bulletsToRemove.Add(bullet);
            }
        }

        foreach (var bullet in bulletsToRemove)
        {
            bullets.Remove(bullet);
            var mainWindow = Application.Current.MainWindow;
            var canvas = mainWindow.FindName("GameCanvas") as Canvas;
            canvas.Children.Remove(bullet.Element);
        }
    }

    private async void StartGameLoop()
    {
        const int targetFps = 60;
        var targetFrameTime = TimeSpan.FromSeconds(1.0 / targetFps);

        DateTime lastFrameTime = DateTime.Now;  

        while (isGameRunning)
        {
            var elapsed = DateTime.Now - lastFrameTime;

            if (elapsed < targetFrameTime)
            {
                await Task.Delay(targetFrameTime - elapsed);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                GameLoop(null, EventArgs.Empty);
            });

            lastFrameTime = DateTime.Now;
        }
    }



    public void StartGame()
    {
        StartGameLoop();
    }

    private void GameLoop(object sender, EventArgs e)
    {
        if (IsPause) return;

        if (!(Application.Current.MainWindow is MainWindow mainWindow)) return;
        if (!(mainWindow.FindName("Car") is Image)) return;

        var MainWindow = Application.Current.MainWindow;
        var canvas = MainWindow.FindName("GameCanvas") as Canvas;

        Car.MoveToTop(canvas);
        Car.MoveToBottom(canvas);
        Car.MoveToLeft(canvas, turnSpeed);
        Car.MoveToRight(canvas, turnSpeed);

        foreach (var enemy in enemies)
        {
            enemy.Move(EnemySpeed, enemies);
        }

        foreach (var obstacle in obstacles)
        {
            obstacle.Move(Speed);
        }

        foreach (var coin in coins)
        {
            coin.Move(Speed);
        }

        foreach (var powerUp in powerUps)
        {
            powerUp.Move(Speed);
        }

        foreach (var tree in Trees)
        { 
            tree.Move(Speed, Trees);
        }

        var bulletsToRemove = new List<Bullet>();
        foreach (var bullet in bullets)
        {
            bullet.Move(Speed);

            if (bullet.Coords.Y < 0)
            {
                bulletsToRemove.Add(bullet);
            }
        }

        foreach (var bullet in bulletsToRemove)
        {
            canvas.Children.Remove(bullet.Element);
            bullets.Remove(bullet);
        }

        var bulletsToRemoveOnCollision = new List<Bullet>();
        foreach (var bullet in bullets)
        {
            foreach (var enemy in enemies)
            {
                if (bullet.Element.Visibility == Visibility.Visible &&
                    enemy.Element.Visibility == Visibility.Visible &&
                    bullet.HasCollision(enemy.Element))
                {
                    Score += 2;
                    enemy.Reset(enemies);
                    bulletsToRemoveOnCollision.Add(bullet);
                    break;
                }
            }
        }


        foreach (var bullet in bulletsToRemoveOnCollision)
        {
            canvas.Children.Remove(bullet.Element);
            bullets.Remove(bullet);
        }

        foreach (var obstacle in obstacles)
        {
            if (obstacle.Element.Visibility == Visibility.Visible &&
                Car.Visibility == Visibility.Visible &&
                Car.HasCollision(obstacle.Element))
            {
                FinishGame();
                return;
            }
        }

        foreach (var enemy in enemies)
        {
            if (enemy.Element.Visibility == Visibility.Visible &&
                Car.Visibility == Visibility.Visible &&
                Car.HasCollision(enemy.Element))
            {
                FinishGame();
                return;
            }
        }


        foreach (var coin in coins)
        {
            if (coin.Element.Visibility == Visibility.Visible &&
                Car.Visibility == Visibility.Visible &&
                Car.HasCollision(coin.Element))
            {
                Score += 1;
                Speed += 0.1;
                turnSpeed += 0.1;
                EnemySpeed += 0.1;
                coin.Reset();
            }
        }
        foreach (var powerUp in powerUps)
        {
            if (powerUp.Element.Visibility == Visibility.Visible &&
                Car.Visibility == Visibility.Visible &&
                Car.HasCollision(powerUp.Element) && !isPowerUpActive)
            {
                isPowerUpActive = true;
                Speed *= 2;
                EnemySpeed *= 2;
                turnSpeed *= 2;

                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                activeTimers.Add(timer);

                timer.Tick += (sender2, args) =>
                {
                    EnemySpeed /= 2;
                    Speed /= 2;
                    turnSpeed /= 2;
                    isPowerUpActive = false;
                    timer.Stop();
                    activeTimers.Remove(timer);
                };
                timer.Start();

                powerUp.Reset();
            }
        }

        foreach (var enemy in enemies)
        {
            foreach (var obstacle in obstacles)
            {
                if (enemy.Element.Visibility == Visibility.Visible &&
                    obstacle.Element.Visibility == Visibility.Visible &&
                    enemy.HasCollision(obstacle.Element))
                {
                    enemy.Reset(enemies);
                    break;
                }
            }
        }
    }


    private void FinishGame()
    {
        IsPause = true;

        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.ExitButton.Visibility = Visibility.Visible;
            mainWindow.ShowPlayerNameGrid(); 
        }
    }



    public void TogglePause()
    {
        isPause = !isPause;
    }

    public void HandleKeyDown(KeyEventArgs e)
    {
        if (IsPause) return;

        var keyMapping = new Dictionary<Key, Action>
        {
            { Key.Up, () => Car.IsMovingTop = true },
            { Key.Down, () => Car.IsMovingBottom = true },
            { Key.Left, () => Car.IsMovingLeft = true },
            { Key.Right, () => Car.IsMovingRight = true },
            { Key.Space, () => FireBullet(Car.Coords.X + Car.Width / 2 - 5, Car.Coords.Y) }
        };

        if (keyMapping.ContainsKey(e.Key))
        {
            keyMapping[e.Key].Invoke();
        }
    }

    public void HandleKeyUp(KeyEventArgs e)
    {
        var keyMapping = new Dictionary<Key, Action>
        {
            { Key.Up, () => Car.IsMovingTop = false },
            { Key.Down, () => Car.IsMovingBottom = false },
            { Key.Left, () => Car.IsMovingLeft = false },
            { Key.Right, () => Car.IsMovingRight = false }
        };

        if (keyMapping.ContainsKey(e.Key))
        {
            keyMapping[e.Key].Invoke();
        }
    }



    public void RestartGame()
    {
        isGameRunning = false;  
        Score = 0;
        Speed = 2;
        EnemySpeed = GetEnemySpeed(difficulty);
        turnSpeed = 4;

        enemies.Clear();
        obstacles.Clear();
        coins.Clear();
        powerUps.Clear();
        bullets.Clear();

        Enemies = new List<Enemy>();
        Obstacles = new List<Obstacle>();
        Coins = new List<Coin>();
        PowerUps = new List<PowerUp>();

        car.ResetToInitialPosition();


        var mainWindow = Application.Current.MainWindow;

        if (mainWindow.FindName("GameCanvas") is Canvas canvas)
        {
            var dynamicElements = canvas.Children.OfType<FrameworkElement>()
                .Where(element => !element.Name.StartsWith("Tree") && element.Tag != null && element.Tag.ToString() == "dynamic").ToList();

            foreach (var element in dynamicElements)
            {
                canvas.Children.Remove(element);
            }
            foreach (var tree in trees)
            {
                tree.PositionTreeOutsideRoad(trees);
                tree.MoveElement();
            }


        }
        foreach (var timer in activeTimers)
        {
            timer.Stop();
        }
        activeTimers.Clear();

        InitializeGameObjects();
        isPause = false;
        StartGame();

    }




    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

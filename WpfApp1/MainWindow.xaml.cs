using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GameApp.Models;
using Models;

namespace GameApp
{
    public partial class MainWindow : Window
    {
        private readonly GameViewModel viewModel;
        private bool isGameStarted = false;
        private readonly DispatcherTimer scoreTimer;
        private const int baseScoreIncrementInterval = 1000;
        private int scoreBuffer = 0;
        private int tickCounter = 0;
        private bool isNameSaved = false;


        public MainWindow()
        {
            InitializeComponent();
            viewModel = new GameViewModel("easy");
            DataContext = viewModel;
            GameGrid.Visibility = Visibility.Collapsed;
            LeaderboardGrid.Visibility = Visibility.Collapsed;
            MenuGrid.Visibility = Visibility.Visible;
            this.Loaded += MainWindow_Loaded;
            BackgroundVideo.MediaOpened += BackgroundVideo_MediaOpened;
            BackgroundVideo.MediaEnded += BackgroundVideo_MediaEnded;
            scoreTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(baseScoreIncrementInterval)
            };
            scoreTimer.Tick += ScoreTimer_Tick;
            scoreTimer.Start();

        }

        private async void ScoreTimer_Tick(object sender, EventArgs e)
        {
            if (!viewModel.IsPause && isGameStarted)
            {
                tickCounter++;
                if (tickCounter >= 10)
                {
                    await Task.Run(() =>
                    {
                        int scoreIncrement = CalculateScoreIncrement();
                        scoreBuffer += scoreIncrement;
                    });

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        viewModel.Score += scoreBuffer;
                        scoreBuffer = 0;
                    });
                    tickCounter = 0;
                }
            }
        }


        private int CalculateScoreIncrement()
        {
            int scoreIncrement = (int)Math.Ceiling(viewModel.Speed * .1);
            return scoreIncrement;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundVideo.Play();
        }

        private void BackgroundVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            BlackScreen.Visibility = Visibility.Collapsed;
            BackgroundVideo.Play();
        }

        private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            BackgroundVideo.Position = TimeSpan.Zero;
            BackgroundVideo.Play();
        }

        private void TogglePauseImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.TogglePause();
            UpdatePauseButtonImage();
        }

        private void UpdatePauseButtonImage()
        {
            if (viewModel.IsPause)
            {
                TogglePauseImage.Source = new BitmapImage(new Uri("images/play.png", UriKind.Relative));
            }
            else
            {
                TogglePauseImage.Source = new BitmapImage(new Uri("images/pause.png", UriKind.Relative));
            }
        }

        private void LeaderBoard_Click(object sender, RoutedEventArgs e)
        {
            MenuGrid.Visibility = Visibility.Collapsed;
            LeaderboardGrid.Visibility = Visibility.Visible;
            LeaderboardManager manager = new LeaderboardManager();
            LeaderboardDataGrid.ItemsSource = manager.Entries;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameStarted)
            {
                viewModel.HandleKeyDown(e);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (isGameStarted)
            {
                viewModel.HandleKeyUp(e);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuGrid.Visibility = Visibility.Collapsed;
                LeaderboardGrid.Visibility = Visibility.Collapsed;
                GameGrid.Visibility = Visibility.Visible;

                isGameStarted = true;
                viewModel.isGameRunning = true;
                viewModel.StartGame();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске игры: {ex.Message}");
            }
        }


        public void ShowPlayerNameGrid()
        {
            PlayerNameGrid.Visibility = Visibility.Visible;
            PlayerNameTextBox.Text = string.Empty;
            PlayerScoreTextBlock.Text = viewModel.Score.ToString();
        }

        private void SavePlayerName_Click(object sender, RoutedEventArgs e)
        {
            if (isNameSaved)
            {
                PlayerMessageTextBlock.Text = "Ваше имя уже сохранено!";
                PlayerMessageTextBlock.Foreground = Brushes.Red;
                return;
            }

            string playerName = PlayerNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(playerName))
            {
                PlayerMessageTextBlock.Text = "Имя не может быть пустым!";
                PlayerMessageTextBlock.Foreground = Brushes.Red;
                return;
            }

            LeaderboardManager manager = new LeaderboardManager();
            manager.AddEntry(new Leaderboard(playerName, viewModel.Score));
            isNameSaved = true;

            PlayerMessageTextBlock.Text = $"Игрок: {playerName} Ваш результат сохранен!";
            PlayerMessageTextBlock.Foreground = Brushes.Green;
            viewModel.RestartGame();
        }

        private void CancelPlayerName_Click(object sender, RoutedEventArgs e)
        {
            PlayerNameGrid.Visibility = Visibility.Collapsed;
            GameGrid.Visibility = Visibility.Collapsed;
            MenuGrid.Visibility = Visibility.Visible;
            viewModel.RestartGame();
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            LeaderboardGrid.Visibility = Visibility.Collapsed;
            MenuGrid.Visibility = Visibility.Visible;
        }

        private void TruncateTable_Click(object sender, RoutedEventArgs e)
        {
            LeaderboardManager manager = new LeaderboardManager();
            manager.ClearLeaderboard();  
            LeaderboardDataGrid.ItemsSource = manager.Entries;  
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            MenuGrid.Visibility = Visibility.Collapsed;
            LeaderboardGrid.Visibility = Visibility.Collapsed;
            PlayerNameGrid.Visibility = Visibility.Collapsed;
            viewModel.RestartGame();
            viewModel.isGameRunning = true;
            GameGrid.Focus();
            Keyboard.Focus(GameGrid);

        }

    }
}

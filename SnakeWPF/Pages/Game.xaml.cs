using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Common;
using System.Collections.Generic;
using System.Linq;

namespace SnakeWPF.Pages
{
    public partial class Game : Page
    {
        public int StepCadr = 0;

        private Dictionary<int, List<Snakes.Point>> previousSnakePositions = new Dictionary<int, List<Snakes.Point>>();
        private Dictionary<int, List<Snakes.Point>> currentSnakePositions = new Dictionary<int, List<Snakes.Point>>();
        private int interpolationFrame = 0;
        private const int totalInterpolationFrames = 10;
        private System.Windows.Threading.DispatcherTimer animationTimer;

        public Game()
        {
            InitializeComponent();

            animationTimer = new System.Windows.Threading.DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(10); // 100 FPS
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (interpolationFrame < totalInterpolationFrames)
            {
                interpolationFrame++;
                RenderInterpolatedFrame();
            }
        }

        public void CreateUI()
        {
            Dispatcher.Invoke(() =>
            {
                // Копируем текущие позиции в предыдущие
                previousSnakePositions = CloneAllPositions(currentSnakePositions);

                // Сохраняем новые позиции
                currentSnakePositions.Clear();

                if (MainWindow.mainWindow.ViewModelGames != null &&
                    MainWindow.mainWindow.ViewModelGames.SnakesPlayers != null &&
                    MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points != null)
                {
                    int id = MainWindow.mainWindow.ViewModelGames.IdSnake;
                    currentSnakePositions[id] = ClonePoints(MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points);
                }

                if (MainWindow.mainWindow.AllViewModelGames != null)
                {
                    foreach (var otherPlayer in MainWindow.mainWindow.AllViewModelGames)
                    {
                        if (otherPlayer != null &&
                            otherPlayer.SnakesPlayers != null &&
                            !otherPlayer.SnakesPlayers.GameOver &&
                            otherPlayer.SnakesPlayers.Points != null)
                        {
                            int id = otherPlayer.IdSnake;
                            currentSnakePositions[id] = ClonePoints(otherPlayer.SnakesPlayers.Points);
                        }
                    }
                }

                // Если это первый кадр, копируем в previous
                if (previousSnakePositions.Count == 0)
                {
                    previousSnakePositions = CloneAllPositions(currentSnakePositions);
                }

                // Сбрасываем счётчик интерполяции
                interpolationFrame = 0;
            });
        }

        private void RenderInterpolatedFrame()
        {
            canvas.Children.Clear();

            float t = (float)interpolationFrame / totalInterpolationFrames;

            // Отрисовка змейки игрока
            if (MainWindow.mainWindow.ViewModelGames != null)
            {
                int playerId = MainWindow.mainWindow.ViewModelGames.IdSnake;
                DrawInterpolatedSnake(playerId,
                                     Color.FromArgb(255, 0, 127, 14),
                                     Color.FromArgb(255, 0, 198, 19),
                                     t);

                // Отрисовка яблока (без интерполяции)
                if (MainWindow.mainWindow.ViewModelGames.Points != null)
                {
                    DrawApple(MainWindow.mainWindow.ViewModelGames.Points);
                }
            }

            // Отрисовка всех остальных змеек
            if (MainWindow.mainWindow.AllViewModelGames != null)
            {
                foreach (var otherPlayer in MainWindow.mainWindow.AllViewModelGames)
                {
                    if (otherPlayer != null &&
                        otherPlayer.SnakesPlayers != null &&
                        !otherPlayer.SnakesPlayers.GameOver)
                    {
                        DrawInterpolatedSnake(otherPlayer.IdSnake,
                                             Color.FromArgb(255, 135, 135, 135),
                                             Color.FromArgb(255, 160, 160, 160),
                                             t);
                    }
                }
            }
        }

        private void DrawInterpolatedSnake(int snakeId, Color headColor, Color bodyColor, float t)
        {
            if (!currentSnakePositions.ContainsKey(snakeId))
                return;

            List<Snakes.Point> current = currentSnakePositions[snakeId];
            List<Snakes.Point> previous = previousSnakePositions.ContainsKey(snakeId) ?
                                          previousSnakePositions[snakeId] : current;

            if (current == null || current.Count == 0)
                return;

            // Рисуем от хвоста к голове
            for (int i = current.Count - 1; i >= 0; i--)
            {
                int x, y;

                if (i < previous.Count && previous.Count == current.Count)
                {
                    // Интерполяция
                    x = (int)(previous[i].X + (current[i].X - previous[i].X) * t);
                    y = (int)(previous[i].Y + (current[i].Y - previous[i].Y) * t);
                }
                else
                {
                    // Нет предыдущей позиции или длина изменилась - рисуем напрямую
                    x = current[i].X;
                    y = current[i].Y;
                }

                Brush color = (i == 0) ? new SolidColorBrush(headColor) : new SolidColorBrush(bodyColor);

                Ellipse ellipse = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(x - 10, y - 10, 0, 0),
                    Fill = color,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                canvas.Children.Add(ellipse);
            }
        }

        private List<Snakes.Point> ClonePoints(List<Snakes.Point> points)
        {
            if (points == null) return new List<Snakes.Point>();

            var cloned = new List<Snakes.Point>();
            foreach (var p in points)
            {
                cloned.Add(new Snakes.Point { X = p.X, Y = p.Y });
            }
            return cloned;
        }

        private Dictionary<int, List<Snakes.Point>> CloneAllPositions(Dictionary<int, List<Snakes.Point>> source)
        {
            var result = new Dictionary<int, List<Snakes.Point>>();
            foreach (var kvp in source)
            {
                result[kvp.Key] = ClonePoints(kvp.Value);
            }
            return result;
        }

        private void DrawApple(Snakes.Point applePosition)
        {
            if (applePosition == null)
                return;

            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/Apple.png"));

            Ellipse points = new Ellipse
            {
                Width = 30,
                Height = 30,
                Margin = new Thickness(applePosition.X - 20, applePosition.Y - 20, 0, 0),
                Fill = myBrush
            };

            canvas.Children.Add(points);
        }
    }
}

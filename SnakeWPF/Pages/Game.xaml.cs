using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SnakeWPF.Pages
{
    public partial class Game : Page
    {
        public int StepCadr = 0;

        public Game()
        {
            InitializeComponent();
        }

        public void CreateUI()
        {
            Dispatcher.Invoke(() =>
            {
                var model = MainWindow.mainWindow.ViewModelGames;
                if (model == null || model.SnakesPlayers == null || model.SnakesPlayers.Points == null || canvas == null)
                    return;

                // Если кадр 0 то кадр 1
                if (StepCadr == 0) StepCadr = 1;
                else StepCadr = 0;

                for (int iPoint = model.SnakesPlayers.Points.Count - 1; iPoint >= 0; iPoint--)
                {
                    var SnakePoint = model.SnakesPlayers.Points[iPoint];

                    if (iPoint != 0)
                    {
                        var NextSnakePoint = model.SnakesPlayers.Points[iPoint - 1];
                        if (SnakePoint.X != NextSnakePoint.X)
                        {
                            SnakePoint.Y += (iPoint % 2 == 0) ? (StepCadr == 0 ? -1 : 1) : (StepCadr % 2 == 0 ? 1 : -1);
                        }
                        else if (SnakePoint.Y != NextSnakePoint.Y)
                        {
                            SnakePoint.X += (iPoint % 2 == 0) ? (StepCadr == 0 ? -1 : 1) : (StepCadr % 2 == 0 ? 1 : -1);
                        }
                    }

                    Brush color = new SolidColorBrush(iPoint == 0 ? Color.FromArgb(255, 0, 127, 14) : Color.FromArgb(255, 0, 198, 19));
                    Ellipse ellipse = new Ellipse
                    {
                        Width = 20,
                        Height = 20,
                        Margin = new Thickness(SnakePoint.X - 10, SnakePoint.Y - 10, 0, 0),
                        Fill = color,
                        Stroke = Brushes.Black
                    };
                    canvas.Children.Add(ellipse);
                }

                ImageBrush myBrush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/Apple.png"))
                };
                Ellipse points = new Ellipse
                {
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(model.Points.X - 20, model.Points.Y - 20, 0, 0),
                    Fill = myBrush
                };
                canvas.Children.Add(points);
            });
        }
    }
}

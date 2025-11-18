using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnakeWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        // Кадр отрисовки
        public int StepCadr = 0;

        // Канвас для рисования
        public Canvas canvas;

        public Game()
        {
            InitializeComponent();
        }

        public void CreateUI()
        {
            Dispatcher.Invoke(() =>
            {
                // Если кадр 0 то кадр 1
                if (StepCadr == 0) StepCadr = 1;
                else StepCadr = 0;

                // Чистим интерфейс
                canvas.Children.Clear();

                // Перебираем точки змеи
                for (int iPoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points.Count - 1; iPoint >= 0; iPoint--)
                {
                    // Получаем точку змеи
                    var SnakePoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points[iPoint];
                    // Логика смещения/рисования — как на скриншоте (см. ваш пример)
                    // ...
                    // Цвет для первой/остальной точки
                    Brush Color;
                    if (iPoint == 0)
                        Color = new SolidColorBrush(Color.FromArgb(255, 0, 127, 14)); // тёмно-зелёный
                    else
                        Color = new SolidColorBrush(Color.FromArgb(255, 0, 198, 19)); // светло-зелёный

                    Ellipse ellipse = new Ellipse()
                    {
                        Width = 20,
                        Height = 20,
                        Margin = new Thickness(SnakePoint.X - 10, SnakePoint.Y - 10, 0, 0),
                        Fill = Color,
                        Stroke = Brushes.Black
                    };
                    canvas.Children.Add(ellipse);
                }

                // Отрисовка яблока
                ImageBrush myBrush = new ImageBrush();
                myBrush.ImageSource = new BitmapImage(new Uri($"pack://application:,,,/Image/Apple.png"));
                Ellipse points = new Ellipse()
                {
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(MainWindow.mainWindow.ViewModelGames.Points.X - 20,
                                           MainWindow.mainWindow.ViewModelGames.Points.Y - 20, 0, 0),
                    Fill = myBrush
                };
                canvas.Children.Add(points);
            });
        }

    }
}

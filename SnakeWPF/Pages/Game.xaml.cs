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

                    // Смещение точек змеи
                    if (iPoint != 0)
                    {
                        // Получаем следующую точку змеи
                        var NextSnakePoint = MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points[iPoint - 1];

                        // Если точка находится по горизонтали
                        if (SnakePoint.X > NextSnakePoint.X || SnakePoint.X < NextSnakePoint.X)
                        {
                            // если точка чётная
                            if (iPoint % 2 == 0)
                            {
                                // если кадр чётный
                                if (StepCadr == 0)
                                    SnakePoint.Y -= 1;
                                else
                                    SnakePoint.Y += 1;
                            }
                            else
                            {
                                // если кадр нечётный
                                if (StepCadr % 2 == 0)
                                    SnakePoint.Y += 1;
                                else
                                    SnakePoint.Y -= 1;
                            }
                        }
                        // Если точка находится по вертикали
                        else if (SnakePoint.Y > NextSnakePoint.Y || SnakePoint.Y < NextSnakePoint.Y)
                        {
                            if (iPoint % 2 == 0)
                            {
                                if (StepCadr == 0)
                                    SnakePoint.X -= 1;
                                else
                                    SnakePoint.X += 1;
                            }
                            else
                            {
                                if (StepCadr % 2 == 0)
                                    SnakePoint.X += 1;
                                else
                                    SnakePoint.X -= 1;
                            }
                        }
                    }

                    // Цвет для точки
                    Brush color;
                    if (iPoint == 0)
                        color = new SolidColorBrush(Color.FromArgb(255, 0, 127, 14)); // тёмно-зелёный
                    else
                        color = new SolidColorBrush(Color.FromArgb(255, 0, 198, 19)); // светло-зелёный

                    // Рисуем точку змеи
                    Ellipse ellipse = new Ellipse()
                    {
                        Width = 20,
                        Height = 20,
                        Margin = new Thickness(SnakePoint.X - 10, SnakePoint.Y - 10, 0, 0),
                        Fill = color,
                        Stroke = Brushes.Black
                    };
                    canvas.Children.Add(ellipse);
                }

                // Отрисовка яблока
                ImageBrush myBrush = new ImageBrush();
                myBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/Apple.png"));
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

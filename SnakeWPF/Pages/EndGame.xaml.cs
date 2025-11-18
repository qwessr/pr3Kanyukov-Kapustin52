using System.Windows;
using System.Windows.Controls;

namespace SnakeWPF.Pages
{
    public partial class EndGame : Page
    {
        public EndGame()
        {
            InitializeComponent();

            if (MainWindow.mainWindow.ViewModelUserSettings != null && MainWindow.mainWindow.ViewModelGames != null)
            {
                // выводим имя игрока
                name.Content = MainWindow.mainWindow.ViewModelUserSettings.Name;
                // выводим топ игрока
                top.Content = MainWindow.mainWindow.ViewModelGames.Top;
                // выводим очки
                glasses.Content = $"{MainWindow.mainWindow.ViewModelGames.SnakesPlayers.Points.Count - 3} glasses";
            }

            // закрываем соединение
            MainWindow.mainWindow.receivingUdpClient?.Close();
            // останавливаем поток
            MainWindow.mainWindow.tRec?.Abort();
            // обнуляем данные
            MainWindow.mainWindow.ViewModelGames = null;
        }


        private void OpenHome(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(MainWindow.mainWindow.Home);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json;

namespace SnakeWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Старт игры: проверка данных, запуск обмена, отправка данных на сервер
        /// </summary>
        private void StartGame(object sender, RoutedEventArgs e)
        {
            // Если есть соединение, закрываем старое
            if (MainWindow.mainWindow.receivingUdpClient != null)
                MainWindow.mainWindow.receivingUdpClient.Close();
            // Останавливаем поток, если он был
            if (MainWindow.mainWindow.tRec != null)
                MainWindow.mainWindow.tRec.Abort();

            IPAddress UserIPAddress;
            if (!IPAddress.TryParse(ip.Text, out UserIPAddress))
            {
                MessageBox.Show("Please use the IP address in the format X.X.X.X.");
                return;
            }

            int UserPort;
            if (!int.TryParse(port.Text, out UserPort))
            {
                MessageBox.Show("Please use the port as a number.");
                return;
            }

            // Запускаем поток на прослушку
            MainWindow.mainWindow.StartReceiver();
            // Заполняем IP, порт и имя в модель
            MainWindow.mainWindow.ViewModelUserSettings.IPAddress = ip.Text;
            MainWindow.mainWindow.ViewModelUserSettings.Port = port.Text;
            MainWindow.mainWindow.ViewModelUserSettings.Name = name.Text;
            // Отправляем команду /start и данные игрока
            MainWindow.Send("/start|" + JsonConvert.SerializeObject(MainWindow.mainWindow.ViewModelUserSettings));
        }

    }
}

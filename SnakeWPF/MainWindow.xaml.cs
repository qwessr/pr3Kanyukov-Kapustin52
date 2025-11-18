using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Common;

namespace SnakeWPF
{
    /// <summary>
    /// Главное окно, используется для для общения между страницами
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Главная страница, используется для навигации
        /// </summary>
        public static MainWindow mainWindow;

        /// <summary>
        /// Модель данных для передачи IP-адреса устройства, порта и никнейма
        /// </summary>
        public ViewModelUserSettings ViewModelUserSettings = new ViewModelUserSettings();

        /// <summary>
        /// Модель игрока с координатами змейки и игровых объектов
        /// </summary>
        public ViewModelGames ViewModelGames = null;

        /// <summary>
        /// Удалённый IP-адрес для подключения к серверу
        /// </summary>
        public static IPAddress remoteIPAddress = IPAddress.Parse("127.0.0.1");

        /// <summary>
        /// Удалённый порт для подключения к серверу
        /// </summary>
        public static int remotePort = 5001;

        /// <summary>
        /// Фоновый поток для получения данных о игре
        /// </summary>
        public Thread tRec;

        /// <summary>
        /// UDP клиент для получения данных
        /// </summary>
        public UdpClient receivingUdpClient;

        /// <summary>
        /// Страница HOME
        /// </summary>
        public Pages.Home Home = new Pages.Home();

        /// <summary>
        /// Страница GAME
        /// </summary>
        public Pages.Game Game = new Pages.Game();

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Начинаем слушать ответы от сервера
        /// </summary>
        public void StartReceiver()
        {
            tRec = new Thread(new ThreadStart(Receiver));
            // Запускаем поток
            tRec.Start();
        }

    }
}

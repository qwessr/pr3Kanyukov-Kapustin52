using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
            // Запоминаем MainWindow в переменную для обращения
            mainWindow = this;
            // Открываем начальную страницу Home
            OpenPage(Home);
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

        /// <summary>
        /// Открываем страницу с плавной анимацией перехода
        /// </summary>
        public void OpenPage(Page PageOpen)
        {
            // Создаём анимацию
            DoubleAnimation startAnimation = new DoubleAnimation();
            // Задаём начальное значение анимации
            startAnimation.From = 1;
            // Задаём конечное значение анимации
            startAnimation.To = 0;
            // Задаём время анимации
            startAnimation.Duration = TimeSpan.FromSeconds(0.6);
            // Подписываемся на выполнение анимации
            startAnimation.Completed += delegate
            {
                // Переключаем страницу
                MainFrame.Navigate(PageOpen);

                // Создаём конечную анимацию
                DoubleAnimation endAnimation = new DoubleAnimation();
                // Задаём начальное значение анимации
                endAnimation.From = 0;
                // Задаём конечное значение анимации
                endAnimation.To = 1;
                // Задаём время анимации
                endAnimation.Duration = TimeSpan.FromSeconds(0.6);
                // Воспроизводим анимацию на MainFrame, анимация прозрачности
                MainFrame.BeginAnimation(OpacityProperty, endAnimation);
            };

            // Воспроизводим анимацию на MainFrame, анимация прозрачности
            MainFrame.BeginAnimation(OpacityProperty, startAnimation);
        }

        /// <summary>
        /// Прослушиваем канал
        /// </summary>
        public void Receiver()
        {
            // Создаём клиент для прослушивания
            receivingUdpClient = new UdpClient(int.Parse(ViewModelUserSettings.Port));
            // Конечная сетевая точка
            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                // Слушаем постоянно
                while (true)
                {
                    // Ожидание дейтаграммы
                    byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    // Преобразуем и отображаем данные
                    string returnData = Encoding.UTF8.GetString(receiveBytes);

                    // Если у нас не существует данных от сервера (значит мы только что закончили игру)
                    if (ViewModelGames == null)
                    {
                        // Говорим что выполняем вне потока
                        Dispatcher.Invoke(() =>
                        {
                            // Открываем окно с игрой
                            OpenPage(Game);
                        });
                    }

                    // Конвертируем данные в модель
                    ViewModelGames = JsonConvert.DeserializeObject<ViewModelGames>(returnData.ToString());

                    // Если игрок проиграл
                    if (ViewModelGames.SnakesPlayers.GameOver)
                    {
                        // Выполняем вне потока
                        Dispatcher.Invoke(() =>
                        {
                            // Открываем окно с окончанием игры
                            OpenPage(new Pages.EndGame());
                        });
                    }
                    else
                    {
                        // Вызываем создание UI
                        Game.CreateUI();
                    }
                }
            }
            catch (Exception ex)
            {
                // если что-то пошло не по плану, выводим ошибку в консоль проекта
                Debug.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }

        /// <summary>
        /// Отправляем команды серверу по UDP
        /// </summary>
        public static void Send(string datagram)
        {
            // Создаём UdpClient
            UdpClient sender = new UdpClient();
            // Создаём endPoint по информации об удалённом хосте
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);

            try
            {
                // Преобразуем данные в массив байтов
                byte[] bytes = Encoding.UTF8.GetBytes(datagram);
                // Отправляем данные
                sender.Send(bytes, bytes.Length, endPoint);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
            finally
            {
                // Закрыть соединение
                sender.Close();
            }
        }
        /// <summary>
        /// Управление змеёй по клавишам
        /// </summary>
        private void EventKeyUp(object sender, KeyEventArgs e)
        {
            // Проверяем, что у игрока есть IP, порт, данные о змее и игра не закончена
            if (!string.IsNullOrEmpty(ViewModelUserSettings.IPAddress) &&
                !string.IsNullOrEmpty(ViewModelUserSettings.Port) &&
                (ViewModelGames != null && !ViewModelGames.SnakesPlayers.GameOver))
            {
                if (e.Key == Key.Up)
                    Send($"Up|{JsonConvert.SerializeObject(ViewModelUserSettings)}");
                else if (e.Key == Key.Down)
                    Send($"Down|{JsonConvert.SerializeObject(ViewModelUserSettings)}");
                else if (e.Key == Key.Left)
                    Send($"Left|{JsonConvert.SerializeObject(ViewModelUserSettings)}");
                else if (e.Key == Key.Right)
                    Send($"Right|{JsonConvert.SerializeObject(ViewModelUserSettings)}");
            }
        }

        /// <summary>
        /// Выход из приложения
        /// </summary>
        private void QuitApplication(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // закрываем соединение
            if (receivingUdpClient != null)
                receivingUdpClient.Close();
            // останавливаем поток
            if (tRec != null)
                tRec.Abort();
        }

    }


}

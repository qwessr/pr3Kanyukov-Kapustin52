using System.Net;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace SnakeWPF.Pages
{
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.receivingUdpClient?.Close();
            MainWindow.mainWindow.tRec?.Abort();

            if (!IPAddress.TryParse(ip.Text, out IPAddress _))
            {
                MessageBox.Show("Please use the IP address in the format X.X.X.X.");
                return;
            }

            if (!int.TryParse(port.Text, out int _))
            {
                MessageBox.Show("Please use the port as a number.");
                return;
            }

            MainWindow.mainWindow.StartReceiver();
            MainWindow.mainWindow.ViewModelUserSettings.IPAddress = ip.Text;
            MainWindow.mainWindow.ViewModelUserSettings.Port = port.Text;
            MainWindow.mainWindow.ViewModelUserSettings.Name = name.Text;
            MainWindow.Send("/start|" + JsonConvert.SerializeObject(MainWindow.mainWindow.ViewModelUserSettings));
        }
    }
}

using System.Collections.Generic;
using System.Net;
using System.Windows;

namespace P2PChat
{
    /// <summary>
    /// Логика взаимодействия для HostsForm.xaml
    /// </summary>
    public partial class HostsForm : Window
    {
        public IPAddress hostIp { get; set; }
        private List<IPAddress> hostsList;

        public HostsForm()
        {
            InitializeComponent();
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            hostIp = hostsList[lbHosts.SelectedIndex];
            this.Hide();

            MainWindow mainWindow = new MainWindow(hostIp);
            mainWindow.ShowDialog();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hostsList = IpConfig.GetAllLocalHosts();
            lbHosts.ItemsSource = hostsList;
        }
    }
}

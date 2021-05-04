using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Net;

namespace P2PChat
{
    public partial class MainWindow : Window
    {
        private static Random random = new Random();
        private IPAddress selectedIP;
        private Connection connection;

        public MainWindow(IPAddress IP)
        {
            InitializeComponent();

            selectedIP = IP;
            connection = new Connection(UpdateChat);
            connection.UserIP = selectedIP;
            connection.ConnectionToChat(RandomString(random.Next(3, 7)));
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string currMess = txtbxMessage.Text;
            connection.SendNormalMessage(currMess);
            txtbxMessage.Text = "";
            txtbxMessage.Focus();
        }

        private void UpdateChat(string text)
        {
            lbChatWindow.Items.Add(text);
        }

        private void formMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connection.SendDisconnectMessage();
            System.Environment.Exit(0);
        }
    }
}

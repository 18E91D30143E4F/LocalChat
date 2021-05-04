using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows;

namespace P2PChat
{
    class Connection
    {

        private string UserLogin;

        private List<Client> clients = new List<Client>();
        public IPAddress UserIP;
        public delegate void UpdateWindowChat(string text);
        public UpdateWindowChat updateChat;
        private StringBuilder chatHistory;

        private DateTime currentTime;
        private string curTimeStr;

        private SynchronizationContext synchronizationContext;

        private int UDP_PORT = 333;
        private int TCP_PORT = 444;

        public Connection(UpdateWindowChat del)
        {
            updateChat = del;
            chatHistory = new StringBuilder();
            currentTime = new DateTime();
            synchronizationContext = SynchronizationContext.Current;
        }

        public void ConnectionToChat(string login)
        {
            IPEndPoint srcIP = new IPEndPoint(UserIP, UDP_PORT);
            IPEndPoint destIP = new IPEndPoint(IpConfig.CountBroadcastIPINV(UserIP).Address, UDP_PORT);
            UdpClient udpClient = new UdpClient(srcIP);
            udpClient.EnableBroadcast = true;

            UserLogin = login;
            byte[] connectMessBytes = Encoding.UTF8.GetBytes(login);

            try
            {
                udpClient.Send(connectMessBytes, connectMessBytes.Length, destIP);
                udpClient.Close();

                currentTime = DateTime.Now;
                string connectMess = string.Format("{0} <{1}> подключился к чату\n", currentTime.ToLongTimeString(), login);
                chatHistory.Append(connectMess);

                updateChat(string.Format("{0} Вы <{1}> подключились к чату", currentTime.ToLongTimeString(), login));

                Task recieveUdpBroadcast = new Task(ReceiveBroadcast);
                recieveUdpBroadcast.Start();

                Task recieveTCP = new Task(ReceiveTCP);
                recieveTCP.Start();
            }
            catch
            {
                MessageBox.Show("Sending Error!", "BAD", MessageBoxButton.OKCancel);
            }
        }

        private void ReceiveBroadcast()
        {
            IPEndPoint srcIP = new IPEndPoint(UserIP, UDP_PORT);
            IPEndPoint destIP = new IPEndPoint(IPAddress.Any, UDP_PORT);
            UdpClient udpReceiver = new UdpClient(srcIP);

            while (true)
            {
                byte[] receivedData = udpReceiver.Receive(ref destIP);
                string clientLogin = Encoding.UTF8.GetString(receivedData);

                Client newClient = new Client(clientLogin, destIP.Address, TCP_PORT);

                newClient.EstablishConnection();
                clients.Add(newClient);
                newClient.SendMessage(new Message(Message.CONNECT, UserLogin));

                currentTime = DateTime.Now;
                string infoMess = string.Format("{0} <{1}> подключился к чату", currentTime.ToLongTimeString(), newClient.login);

                synchronizationContext.Post(delegate { updateChat(infoMess); }, null);


                Task.Factory.StartNew(() => ListenClient(newClient));
            }
        }

        private void ReceiveTCP()
        {
            TcpListener tcpListener = new TcpListener(UserIP, TCP_PORT);
            tcpListener.Start();

            while (true)
            {
                TcpClient tcpNewClient = tcpListener.AcceptTcpClient();
                Client newClient = new Client(tcpNewClient, TCP_PORT);

                Task.Factory.StartNew(() => ListenClient(newClient));
            }

        }

        private void ListenClient(Client client)
        {
            while (true)
            {
                Message tcpMessage = client.ReceiveMessage();
                string infoMes;

                currentTime = DateTime.Now;
                infoMes = currentTime.ToLongTimeString();

                switch (tcpMessage.code)
                {
                    case Message.CONNECT:
                        client.login = tcpMessage.data;
                        clients.Add(client);
                        GetHistoryMessageToConnect(client);
                        break;

                    case Message.MESSAGE:
                        infoMes += string.Format(" <{0}> {1}\n", client.login, tcpMessage.data);
                        synchronizationContext.Post(delegate { updateChat(infoMes); chatHistory.Append(infoMes); }, null);
                        break;

                    case Message.DISCONNECT:
                        infoMes += string.Format(" <{0}> покинул чат\n", client.login);
                        synchronizationContext.Post(delegate { updateChat(infoMes); chatHistory.Append(infoMes); }, null);
                        clients.Remove(client);
                        return;

                    case Message.GET_HISTORY:
                        SendHistoryMessage(client);
                        break;

                    case Message.SHOW_HISTORY:
                        synchronizationContext.Post(delegate { updateChat(tcpMessage.data); chatHistory.Append(tcpMessage.data); }, null);
                        break;

                    default:
                        break;
                }
            }
        }

        public void SendHistoryMessage(Client client)
        {
            Message historyMessage = new Message(Message.SHOW_HISTORY, chatHistory.ToString());
            client.SendMessage(historyMessage);
        }

        public void GetHistoryMessageToConnect(Client client)
        {
            Message historyMessage = new Message(Message.GET_HISTORY, "");
            client.SendMessage(historyMessage);
        }

        public void SendDisconnectMessage()
        {
            string disconnectStr = string.Format("<{0}> покинул чат", UserLogin);
            Message disconnectMes = new Message(Message.DISCONNECT, disconnectStr);
            SendMessageToAllClients(disconnectMes);
            clients.Remove(clients.Find(CLIENT => UserLogin == CLIENT.login));
        }

        public void SendNormalMessage(string mes)
        {
            if (mes != "")
            {
                Message normalMess = new Message(Message.MESSAGE, mes);
                SendMessageToAllClients(normalMess);
            }
        }

        public void SendMessageToAllClients(Message tcpMes)
        {
            foreach (var user in clients)
            {
                user.SendMessage(tcpMes);
            }

            if (tcpMes.code == Message.MESSAGE)
            {
                currentTime = DateTime.Now;
                curTimeStr = currentTime.ToLongTimeString();

                string infoMessage = string.Format("{0} Вы: {1}", curTimeStr, tcpMes.data);

                updateChat(infoMessage);

                infoMessage = string.Format("{0} <{1}> {2}\n", curTimeStr, UserLogin, tcpMes.data);

                chatHistory.Append(infoMessage);
            }

        }
    }
}

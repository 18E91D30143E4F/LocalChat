using System.Text;
using System.Net;
using System.Net.Sockets;

namespace P2PChat
{
    public class Client
    {
        public string login;
        public IPAddress IP;
        private IPEndPoint endIP;
        private TcpClient tcpClient;
        private int tcpPort;
        public NetworkStream messStream;

        public Client(TcpClient clTcpClient, int clPort)
        {
            tcpClient = clTcpClient;
            tcpPort = clPort;
            IP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
            messStream = tcpClient.GetStream();
        }

        public Client(string clLogin, IPAddress clIP, int clPort)
        {
            login = clLogin;
            IP = clIP;
            tcpPort = clPort;
            endIP = new IPEndPoint(IP, tcpPort);
        }

        public void EstablishConnection()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(endIP);
            messStream = tcpClient.GetStream();
        }

        public void SendMessage(Message clMess)
        {
            byte[] bMess = Encoding.UTF8.GetBytes((char)clMess.code + clMess.data);
            messStream.Write(bMess, 0, bMess.Length);
        }

        public Message ReceiveMessage()
        {
            StringBuilder message = new StringBuilder();
            byte[] buff = new byte[1024];

            do
            {
                try
                {
                    int size = messStream.Read(buff, 0, buff.Length);
                    message.Append(Encoding.UTF8.GetString(buff, 0, size));
                }
                catch
                {
                    return new Message(Message.DISCONNECT, "");
                }

            }
            while (messStream.DataAvailable);

            Message recvMess = new Message(message[0], message.ToString().Substring(1));

            return recvMess;
        }

    }
}

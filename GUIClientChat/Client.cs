using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUIClientChat
{
    class Client
    {
        private string IP;
        private int PORT;
        private TcpClient client;
        private NetworkStream stream;
        private Form1 form;
        public string userName;

        public Client(string userName, string IP, int PORT, Form1 form)
        {
            this.form = form;
            this.userName = userName;
            this.IP = IP;
            this.PORT = PORT;
        }

        public void Start()
        {
            client = new TcpClient();
            try
            {
                client.Connect(IP, PORT);
                stream = client.GetStream();

                //В качестве первого сообщения отправляем на сервер
                //имя пользователя
                byte[] msgBytes = Encoding.Unicode.GetBytes(userName);
                stream.Write(msgBytes, 0, msgBytes.Length);

                Thread dataThread = new Thread(new ThreadStart(RecieveMessage));
                dataThread.Start();
                dataThread.IsBackground = true;

                form.richTextBox1.Text = String.Format("Session is started ... \nYOUR NICK: {0}", userName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Disconnect();
            }
        }
      

        //Отправка сообщений (происходит в главном потоке)
        public void SendMessage()
        {
            string msg = form.textBox2.Text;
            try
            {
                if (!(msg.Replace(" ", string.Empty).Equals("") || (msg == null)))
                {
                    byte[] msgBytes = Encoding.Unicode.GetBytes(msg);
                    stream.Write(msgBytes, 0, msgBytes.Length);
                }
            } catch
            {
                form.richTextBox1.Text = "Server is unavailable. Please try again";
            }
        }

        //Прием сообщений (происходит в дополнительном потоке)
        void RecieveMessage()
        {
            while (client.Connected)
            {
                try
                {
                    byte[] msgBytes = new byte[128];
                    StringBuilder msg = new StringBuilder();
                    int bytes = 0;

                    do
                    {
                        bytes = stream.Read(msgBytes, 0, msgBytes.Length);
                        msg.Append(Encoding.Unicode.GetString(msgBytes, 0, bytes));
                    } while (stream.DataAvailable);

                    form.Invoke(new Action(() => form.richTextBox1.Text = form.richTextBox1.Text + "\n" + msg.ToString()));
                }
                catch (Exception e)
                {
                    //Console.WriteLine("Error. Connection faild");
                    //disconnect();
                }
            }
        }

        public void DisconnectForCurrentClient()
        {
            if (stream != null) stream.Close();
            if (client != null) client.Close();
        }

        public void Disconnect()
        {
            DisconnectForCurrentClient();
            Environment.Exit(0);
        }
    }
}

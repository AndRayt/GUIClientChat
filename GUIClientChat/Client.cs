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
                form.richTextBox1.Text = "Server is unavailable. Please try again later";
                //Disconnect();
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
                form.richTextBox1.Text = "Server is unavailable. Please try again later";
            }
        }

        //Прием сообщений (происходит в дополнительном потоке)
        void RecieveMessage()
        {
            bool isRed = false;
            while (client.Connected)
            {
                try
                {
                    byte[] msgBytes = new byte[128];
                    StringBuilder msgSB = new StringBuilder();
                    int bytes = 0;

                    do
                    {
                        bytes = stream.Read(msgBytes, 0, msgBytes.Length);
                        msgSB.Append(Encoding.Unicode.GetString(msgBytes, 0, bytes));
                    } while (stream.DataAvailable);

                    //вывод списка пользователей
                    if (msgSB[0] == '#')
                    {
                        msgSB.Remove(0, 1);
                        form.Invoke(new Action(() => form.richTextBox2.Text = msgSB.ToString()));
                    }
                    else
                    {
                        //записываем текст
                        form.Invoke(new Action(() => {

                            string msg = msgSB.ToString();

                            if (msg[msg.Length - 1] == '#')
                            {
                                isRed = true;
                                msg = msg.Substring(0, msg.Length - 1);
                            }

                            if (!(form.richTextBox1.Text[form.richTextBox1.Text.Length - 1] == '\n'))
                                form.richTextBox1.AppendText("\n");
                            form.richTextBox1.AppendText(msg);

                            //окрашиваем ники в зеленый
                            foreach (string line in form.richTextBox1.Lines.Skip(2))
                            {
                                form.richTextBox1.SelectionStart = form.richTextBox1.Text.LastIndexOf(line);
                                form.richTextBox1.SelectionLength = line.Split(':')[0].Length;
                                form.richTextBox1.SelectionColor = System.Drawing.Color.Green;
                            }
                            // убираем выделение
                            form.richTextBox1.SelectionLength = 0;
                            // ставим курсор после последнего символа
                            form.richTextBox1.SelectionStart = form.richTextBox1.Text.Length;

                            //проверяем нужно ли окрасить сообщение в карсный цвет
                            if (isRed)
                            {
                                //выделяем текст
                                string prevLine = form.richTextBox1.Lines[form.richTextBox1.Lines.Length - 1];
                                form.richTextBox1.SelectionStart = form.richTextBox1.Text.LastIndexOf(prevLine);
                                form.richTextBox1.SelectionLength = prevLine.Length;
                                //меняем цвет
                                form.richTextBox1.SelectionColor = System.Drawing.Color.Red;
                                // убираем выделение
                                form.richTextBox1.SelectionLength = 0;
                                // ставим курсор после последнего символа
                                form.richTextBox1.SelectionStart = form.richTextBox1.Text.Length;
                                form.richTextBox1.SelectionColor = System.Drawing.Color.Black;
                                isRed = false;
                            }
                        }));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIClientChat
{
    public partial class Form1 : Form
    {
        private const string IP = "127.0.0.1";
        private const int PORT = 8000;
        Client client = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (client != null)
            {
                client.sendMessage();
                textBox2.Clear();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string userName = textBox1.Text;
            label3.Text = userName;

            //удаляем все пробелы
            userName = userName.Replace(" ", string.Empty);
            if (!(userName.Equals("") || userName == null))
            {
                if (client != null)
                {
                    client.disconnectForCurrentClient();
                }
                client = new Client(userName, IP, PORT, this);
                client.start();
            }
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.disconnect();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chatproj
{
    public partial class Form1 : Form
    {
        public static int usercount = 0;
        

        public List<TcpClient> clientlist = new List<TcpClient>();
        private List<string> clientnamelist = new List<string>();


        private static Int32 port ;
        private Byte[] bytes ;
        private String data, data1 = null;
        private static IPAddress localAddr ;
        private TcpListener server;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add("Sameed");
            dataGridView1.Rows.Add("Daniyal");
            dataGridView1.Rows.Add("Kashif");

            //innitialize
            port = 12000;
            bytes = new Byte[256];
            localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, port);


            server.Start();
            Console.WriteLine("Waiting for a connection ... ");
            //serverig();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //serverig1();      
            if (clientnamelist.Contains(dataGridView1.CurrentRow.Cells[0].Value.ToString())) {
                int loc = clientnamelist.IndexOf(dataGridView1.CurrentRow.Cells[0].Value.ToString());
                Console.WriteLine(clientlist[loc]);

                byte[] msg3 = Encoding.ASCII.GetBytes("Server: " + textBox2.Text);
                string tt2 = Encoding.ASCII.GetString(msg3);
                NetworkStream sw;
                TcpClient c3 = clientlist[loc];
                Console.WriteLine("send specific msg  ----  " + tt2 + c3.ToString());
                sw = c3.GetStream();
                sw.Write(msg3, 0, msg3.Length);

                //textBox3.Text = "";
                textBox2.Text = "";
            }
            else
            {
                MessageBox.Show("Client not connected, Please Retry !");
                //textBox3.Text = "";
            }
        }           //extra
        private async void serverig1()
        {
            await Task.Run(serverig);
        }
        private void serverig()
        {
            //clientlist =new List<TcpClient>();

            /*Console.WriteLine("Waiting for a connection... ");
            TcpClient client = server.AcceptTcpClient();
            clientlist.Add(client);     //client list
            Console.WriteLine("Connected! ");*/

            while (true)
            {

                TcpClient client = server.AcceptTcpClient();
                //clientlist.Add(client);     //client list
                Console.WriteLine("Connected! " + client);


                //client input
                data = null;
                NetworkStream stream = client.GetStream();

                string sendto,sender;
                int i;
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {

                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("server Received: {0}", data);
                    (data, sendto) = rcvrname(data);
                    (data,sender) = chkusername(data, client);
                    //(data, sendto) = rcvrname(data);

                    sentclienttoclient(data, sendto, sender);
                    

                    //processing on data
                    //data = data.ToUpper();
                   /* data1 = textBox2.Text;
                    byte[] msg = Encoding.ASCII.GetBytes("Server: "+data1);


                    //broadcast

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent by server: {0}", data1);*/
                    
                    //updaterec(data);
                    SetTextBox(data);
                }

                // Shutdown and end connection
                //client.Close();
            }
            //client.Close(); 

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();

        }

        private void sentclienttoclient(string clientmessage, string sendto, string sender)        ////// 
        {
            byte[] msg = Encoding.ASCII.GetBytes(sender +clientmessage);
            string tt1 = Encoding.ASCII.GetString(msg);
            NetworkStream sw;

            //brosadcast
            if (clientnamelist.Contains(sendto))
            {
                int loc = clientnamelist.IndexOf(sendto);
                Console.WriteLine(clientlist[loc]);

                
                
                TcpClient c3 = clientlist[loc];
                Console.WriteLine("send specific msg  ----  " + tt1 + c3.ToString());
                sw = c3.GetStream();
                sw.Write(msg, 0, msg.Length);

            }
            /*else
            {
                MessageBox.Show("Client not connected, Please Retry !");
                textBox3.Text = "";
            }*/
        }

        private (string, string ) rcvrname(string gotstr)
        {
            string sendto,st1 = "";
            string rcvrcode = ")^$%$^^^&";
            //Console.WriteLine(gotstr.Substring(0, 9));
            if (gotstr.Substring(0, 9) == rcvrcode)
            {
                sendto = gotstr.Substring(9, 10);
                Console.WriteLine("7sendto) " + sendto);
                st1 = sendto.ToString().TrimEnd();
                Console.WriteLine("7sendto) " + st1);

                gotstr = gotstr.Substring(19);
                Console.WriteLine(gotstr);
            }
            return (gotstr , st1);
        }

        private (string,string) chkusername(string gotstr,TcpClient client)
        {
            string sendername="";
            if (gotstr.Substring(0,5)== "&$##*" )
            {
                sendername = gotstr.Substring(5);
                clientlist.Add(client);
                clientnamelist.Add(sendername);

                gotstr = gotstr.Substring(5) + " connected ";
            }
            return (gotstr,sendername);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            client c1 = new client();
            c1.Show();
            serverig1();
        }       //new client

        /*private void button4_Click(object sender, EventArgs e)
        {
            server.Stop();
        }*/           // stop server
        private async void updaterec(string data)
        {
            await Task.Run(()=>SetTextBox(data));
        }       //trial
        public void SetTextBox(String text)
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate () { SetTextBox(text); });
                return;
            }
            textBox1.Text += Environment.NewLine + text;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

      /*  private void button3_Click_1(object sender, EventArgs e)
        {
            client c1 = new client();
            c1.Show();
            serverig1();
        }
*/
       /* private void button3_Click(object sender, EventArgs e)
        {

        }*/

        public void button1_Click(object sender, EventArgs e)
        {
            
            byte[] msg = Encoding.ASCII.GetBytes("Server: "+textBox2.Text);
            string tt1 = Encoding.ASCII.GetString(msg);
            NetworkStream sw;

            //brosadcast
            foreach (TcpClient c1 in clientlist)
            {
                TcpClient c2 = c1;
                Console.WriteLine("broadcast  ----  "+tt1+c1.ToString());
                sw = c2.GetStream();
                sw.Write(msg,0,msg.Length);
            }
            SetTextBox(tt1);
            textBox2.Text = "";
        }           //broadcast
    }
    
}

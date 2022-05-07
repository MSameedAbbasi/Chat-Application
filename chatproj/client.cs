using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chatproj
{
    public partial class client : Form
    {
        public static string username ;
        //private string username2 ;

        private static IPAddress localAddr;
        private static Int32 port;
        private NetworkStream stream;
        private TcpClient clients;

        private string resp1 ;
        
        public client()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.AppendText(Environment.NewLine + "ME : " + textBox1.Text);

            clientsend();
            
        }       //client


        private async void clientsend()
        {
            string temptxt= textBox1.Text;
            textBox1.Text = "";

            await Task.Run(() => cliig(temptxt)) ;
            if (!textBox2.IsDisposed)
            {
                SetTextBoxbcast( resp1);
            }       // username2 +
            /*stream.Close();
            clients.Close();*/
        }//name capital
        public void cliig(string text1)
        {

            string rcvrcode = ")^$%$^^^&";
            string sendernamestring = label2.Text;
            int len1=label2.Text.Length;
            for (int i = 9+ label2.Text.Length; i < 19; i++)        //test it
            {
                sendernamestring += " ";
            }
            Console.WriteLine("sendernamestring"+sendernamestring);
            string message = rcvrcode + sendernamestring + Text + ": " + text1.ToString();      //get input msg

            
            Byte[] data = Encoding.ASCII.GetBytes(message);     //processing
            

                                                   //send data
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Sent: {0}", message);


            data = new Byte[256];               // Receive the TcpServer.response.
            string responseData ;


            //responseData = strreader();         // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);        //process response
            responseData = Encoding.ASCII.GetString(data, 0, bytes);

            Console.WriteLine("Received: {0}", responseData);       //output response
            
            resp1 = responseData;

        }
        public void SetTextBoxbcast(String text)        //speed improve
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate () { SetTextBoxbcast(text); });
                return;
            }
            textBox2.Text += Environment.NewLine + text;
        }

        private async void broadcastreceive()
        {
            await Task.Run(getbmsg);
            if (!textBox2.IsDisposed)
            {
                SetTextBoxbcast(resp1);
            }       //+ username2 
            /*stream.Close();
            clients.Close();*/
        }
        public void getbmsg()
        {

            Byte[] data = new Byte[256];               // Receive the TcpServer.response.
            string responseData;


            Int32 bytes = stream.Read(data, 0, data.Length);        //process response
            responseData = Encoding.ASCII.GetString(data, 0, bytes);

            Console.WriteLine("Broadcast Received: {0}", responseData);       //output response

            resp1 = responseData;

        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //broadcastreceive();         //dobara bcast k lie ye hai
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

       

        private void client_Load(object sender, EventArgs e)
        {

            // TODO: This line of code loads data into the 'database1DataSet.clients' table. You can move, or remove it, as needed.
            //this.clientsTableAdapter.Fill(this.database1DataSet.clients);
            //dataGridView1.Columns.Add("FRIENDS");
            dataGridView1.Rows.Add("Sameed");
            dataGridView1.Rows.Add("Daniyal");
            dataGridView1.Rows.Add("Kashif");

            if (Form1.usercount < 1)
            {
                username = "Sameed";
            }
            else if (Form1.usercount < 2)
            {
                username = "Daniyal";
            }
            else if (Form1.usercount < 3)
            {
                username = "Kashif";
            }
            else
            {
                username = "Anonymous user " + Form1.usercount.ToString();
            }

            Form1.usercount += 1;
            this.Text = username;
            
            localAddr = IPAddress.Parse("127.0.0.1");
            port = 12000;
            clients = new TcpClient(localAddr.ToString(), port);
            stream = clients.GetStream();
            clientnamesend();

            //broadcastreceive();
            brcive();                               //aik hi broadcast ho raha hai!
            Console.WriteLine("load end ///");
        }

        private async void clientnamesend()
        {
            string temptxt = Text;
            await Task.Run(() => clientname(temptxt));
        }
        public void clientname(string text1)
        {
            string checkerkeystr = "&$##*";
            Byte[] data = Encoding.ASCII.GetBytes(checkerkeystr+Text);     //processing

            stream.Write(data, 0, data.Length);
            //Console.WriteLine("Sent name: {0}",Text);

        }


        private async void brcive()
        {
            while (true)
            {
                await Task.Run(brc1);
            }
        }
        private void brc1()
        {
             broadcastreceive();
            
        }


        private void button1_MouseHover(object sender, EventArgs e)
        {
            //button1.BackColor = Color.Azure;    //change
        }
        private void button1_MouseLeave(object sender, EventArgs e)
        {
            //button1.BackColor= Color.PaleTurquoise;
        }

        private void selectchat()
        {
            string n1 = dataGridView1.CurrentRow.Cells[0].Value.ToString();
            label2.Text = n1;
            textBox2.Text="";       //if you want to clear chat box on connection
        }

        private void button2_Click(object sender, EventArgs e)
        {
            selectchat();
        }           //start chat with selected user

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

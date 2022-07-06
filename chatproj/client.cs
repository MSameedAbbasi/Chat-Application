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
        private List<string> clientset;
        private static IPAddress localAddr;
        private static Int32 port;
        private NetworkStream stream;
        private TcpClient clients;

        private string resp1 ;
        
        public client()
        {
            InitializeComponent();
        }
        

        private void button1_Click(object sender, EventArgs e)      //send button
        {
            textBox2.AppendText(Environment.NewLine + "ME : " + textBox1.Text);

            clientsend();
            
        }       //client
        private async void clientsend()
        {
            string temptxt= textBox1.Text;
            textBox1.Text = "";
            Console.WriteLine( "clientsend 1");  


            await Task.Run(() => client_start(temptxt)) ;   //ye line slow ker rahi traansmission
            //cliig(temptxt);


            Console.WriteLine("clientsend 2");
            if (!textBox2.IsDisposed)
            {
                //SetTextBoxbcast( resp1);

                textBox2.Text += Environment.NewLine + resp1;

            }       // username2 +
            /*stream.Close();
            clients.Close();*/
        }//name capital

        public void client_start(string sent_msg)
        {
            Console.WriteLine("client_start");
            string rcvrcode = "$%$";
            string sendernamestring = label2.Text;
            int lenght_of_sender_name=label2.Text.Length;
            for (int i =  lenght_of_sender_name; i < 20; i++)        //test it
            {
                sendernamestring += " ";
            }
            Console.WriteLine("sendernamestring >>"+sendernamestring);
            string message = rcvrcode + sendernamestring + Text + ": " + sent_msg.ToString();      //get input msg
            Console.WriteLine("the sent message is >>>"+message);
            
            Byte[] data = Encoding.ASCII.GetBytes(message);     //processing
            
            stream.Write(data, 0, data.Length);                      //send data
            Console.WriteLine("\n Sent: {0}", message);

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




        private async void brcive()
        {
            while (true)
            {
                await Task.Run(broadcastreceive);
            }
        }
        private async void broadcastreceive()
        {
            await Task.Run(getbmsg);
            if (!textBox2.IsDisposed)
            {
                SetTextBoxbcast(resp1);
            }       
            //+ username2 
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

            if (responseData.Substring(0, 1) == "#")
            {
                clientset = responseData.Split('#').ToList();
                clientset = clientset.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                int lenght= clientset.Count;
                responseData = clientset[ lenght - 1];
                clientset.RemoveAt( lenght - 1 );
                Setdatagridview();
            }

            resp1 = responseData;

        }



        private void Setdatagridview()
        {

            try
            {
                
                if (InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate () { Setdatagridview(); });
                    return;
                }
                dataGridView1.Rows.Clear();
                foreach (string name in clientset)
                {
                    if (name != Text)
                    {
                        dataGridView1.Rows.Add(name);
                    }
                }

            }
            catch
            {
                Console.WriteLine("set datagrid error");
            }

        }


        private void client_Load(object sender, EventArgs e)
        {


            if (server_form.usercount < 1)
            {
                username = "Sameed";
            }
            else if (server_form.usercount < 2)
            {
                username = "Daniyal";
            }
            else if (server_form.usercount < 3)
            {
                username = "Kashif";
            }
            else
            {
                username = "Anonymous user " + server_form.usercount.ToString();
            }

            server_form.usercount += 1;
            this.Text = username;
            
            localAddr = IPAddress.Parse("192.168.0.57");
            port = 12000;
            clients = new TcpClient(localAddr.ToString(), port);
            stream = clients.GetStream();



            clientnamesend();

            //broadcastreceive();
            brcive();                               //aik hi broadcast ho raha hai!
            Console.WriteLine("load end ///");
        }
        private async void clientnamesend() // to register the clients name corresponding to its socket
        {
            string temptxt = Text;
            await Task.Run(() => clientname(temptxt));
        }
        public void clientname(string text1)
        {
            string checkerkeystr = "&$##*";
            Byte[] data = Encoding.ASCII.GetBytes(checkerkeystr+Text);     //processing

            stream.Write(data, 0, data.Length);
            Console.WriteLine("Sent name: {0}",Text);

        }




        private void selectchat()
        {
            string n1 = dataGridView1.CurrentRow.Cells[0].Value.ToString();
            label2.Text = n1;
            textBox2.Text="";       //if you want to clear chat box on connection
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            selectchat();
        }   //start chat with selected user





        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //broadcastreceive();         //dobara bcast k lie ye hai
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        private void button1_MouseHover(object sender, EventArgs e)
        {
            //button1.BackColor = Color.Azure;    //change
        }
        private void button1_MouseLeave(object sender, EventArgs e)
        {
            //button1.BackColor= Color.PaleTurquoise;
        }


    }
}

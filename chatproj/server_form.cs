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
    public partial class server_form : Form
    {
        public static int usercount = 0;
        

        public List<TcpClient> clientlist = new List<TcpClient>();
        private List<string> clientnamelist = new List<string>();
        string client_list_string;

        private static Int32 port ;
        private Byte[] bytes ;
        private String data, data1 = null;
        private static IPAddress localAddr ;
        private TcpListener server;


        public server_form()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setdatagridview();

            //innitialize
            port = 12000;
            bytes = new Byte[256];
            localAddr = IPAddress.Parse("192.168.0.57");
            server = new TcpListener(localAddr, port);


            server.Start();
            Console.WriteLine("Waiting for a connection ... ");
            //serverig();
            serverig1();
        }

        private void button2_Click(object sender, EventArgs e)   //send to one client
        {
            foreach (string name in clientnamelist)
            {
                Console.WriteLine("client list is " + name + "<<");
            }
            //serverig1();      
            if (clientnamelist.Contains(dataGridView1.CurrentRow.Cells[0].Value.ToString())) {
                int loc = clientnamelist.IndexOf(dataGridView1.CurrentRow.Cells[0].Value.ToString());
                Console.WriteLine(clientlist[loc]);

                byte[] msg3 = Encoding.ASCII.GetBytes(client_list_string + "Server: " + textBox2.Text + "\n#");
                
                string tt2 = Encoding.ASCII.GetString(msg3);
                SetTextBox(tt2+" >> "+ dataGridView1.CurrentRow.Cells[0].Value.ToString());
                NetworkStream sw;
                TcpClient c3 = clientlist[loc];
                Console.WriteLine("send to one client msg  ---- " + tt2 + "<<>>"+ c3.ToString());
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
        }          
        private async void serverig1()
        {
            await Task.Run(serverig);
        }
        private void serverig()
        {
            try
            {
                while (true)
                {

                    TcpClient client = server.AcceptTcpClient();
                    //clientlist.Add(client);     //client list
                    Console.WriteLine("Connected! " + client.ToString());


                    //client input
                    data = null;
                    NetworkStream stream = client.GetStream();

                    string sendto, sender;
                    int i;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        data.TrimEnd('\n','\r');
                        Console.WriteLine("server got>>" + data);
                        var tempreturn2 = chkusername(data, client);
                        data = tempreturn2.Item1;
                        sender = tempreturn2.Item2;
                        //(data, sendto) = rcvrname(data);


                        Console.WriteLine("server Received: {0}", data);
                        var tempreturn = rcvrname(data);
                        data = tempreturn.Item1;
                        sendto = tempreturn.Item2;

                        
                        sentclienttoclient(data, sendto, sender);


                        //processing on data
                        //data = data.ToUpper();
                        data1 =  "ACKNOWLEDGE \n#";
                        byte[] msg = Encoding.ASCII.GetBytes(client_list_string + "Server: " + data1);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent by server: //{0}", client_list_string + "Server: " + data1);
                       

                        SetTextBox(data);
                    }

                    //client.Close();
                }
                //client.Close(); 

                Console.WriteLine("\nHit enter to continue...");
                Console.Read();
            }
            catch
            {
                Console.WriteLine("svr: connection loss");
            }

        }

        private void sentclienttoclient(string clientmessage, string sendto, string sender)        ////// 
        {
            Console.WriteLine("client to client" + sendto + "<<msg>>" + clientmessage + "<<sender>>" + sender);
            foreach (string name in clientnamelist)
            {
                Console.WriteLine("client list is " + name +"<len"+ name.Length + "<<"+name.TrimEnd('\n').Length);
            }
            if (clientnamelist.Contains(sendto))
            {
                byte[] msg = Encoding.ASCII.GetBytes(client_list_string + sender + clientmessage + "\n#");
                string tt1 = Encoding.ASCII.GetString(msg);
                NetworkStream sw;
                Console.WriteLine("1"+tt1);
                int loc = clientnamelist.IndexOf(sendto);
                Console.WriteLine("2"+loc +"<<>>"+ clientlist[loc]);

                
                TcpClient c3 = clientlist[loc];
                Console.WriteLine("send specific msg ----" + tt1 + c3.ToString());
                sw = c3.GetStream();
                sw.Write(msg, 0, msg.Length);

            }
            Console.WriteLine("send client to client end//");
            /*else
            {
                MessageBox.Show("Client not connected, Please Retry !");
                textBox3.Text = "";
            }*/
        }

        private Tuple<string, string > rcvrname(string gotstr)
        {
            string sendto,st1 = "";
            string rcvrcode = "$%$";
            Console.WriteLine("rcvr code" + gotstr.Substring(0, 3));
            if (gotstr.Substring(0, 3) == rcvrcode)
            {
                
                sendto = gotstr.Substring(3, 10);


                Console.WriteLine("7sendto) " + sendto);
                st1 = sendto.ToString().TrimEnd();
                Console.WriteLine("8sendto) " + st1+"<<");
                
                gotstr = gotstr.Substring(13);
                Console.WriteLine(gotstr);
            }
            return Tuple.Create(gotstr , st1);
        }

        private Tuple<string,string> chkusername(string gotstr,TcpClient client)
        {
            string sendername="";
            
            if (gotstr.Substring(0,5)== "&$##*" )
            {
                sendername = gotstr.Substring(5);
                sendername = sendername.TrimEnd('\n', '\r');
                if (clientnamelist.Contains(sendername))
                {
                    clientlist[clientnamelist.IndexOf(sendername)]= client;
                }
                else
                {
                    //sendername = gotstr.Substring(5);
                    clientlist.Add(client);
                    clientnamelist.Add(sendername.TrimEnd('\n', '\r'));
                }
                gotstr = sendername + " connected ";
                setdatagridview();
            }
            Console.WriteLine("check username>>"+sendername +"<<"+gotstr);
            return Tuple.Create(gotstr,sendername);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            client c1 = new client();
            c1.Show();
            
            serverig1();
        }       //new client
        public void button1_Click(object sender, EventArgs e)    //broadcast
        {

            byte[] msg = Encoding.ASCII.GetBytes("Server: " + textBox2.Text + "\n");
            string tt1 = Encoding.ASCII.GetString(msg);
            NetworkStream sw;

            //brosadcast
            foreach (TcpClient c1 in clientlist)
            {
                if (c1.Connected)
                {
                    TcpClient c2 = c1;
                    Console.WriteLine("broadcast  ----  " + tt1 + c1.ToString());
                    sw = c2.GetStream();
                    sw.Write(msg, 0, msg.Length);
                }
            }
            //SetTextBox(tt1);

            textBox1.Text += Environment.NewLine + tt1;

            textBox2.Text = "";
        }           //broadcast
        /* private void setdatagridview()
         {

             Setdatagridview();

         }*/
        private void setdatagridview()
        {

            try
            {
                
                Array clientset = clientnamelist.Distinct().ToArray();  //ram
                client_list_string = "#";
                if (InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate () { setdatagridview(); });
                    return;
                }
                dataGridView1.Rows.Clear();
                foreach (string name in clientset)
                {
                    client_list_string += name.ToString().TrimEnd('\n') + "#";
                    dataGridView1.Rows.Add(name);
                    
                }
                //client_list_string += "#";

                Console.WriteLine("array  >>>>" + client_list_string);
            }
            catch
            {
                Console.WriteLine("set datagrid error");
            }
            
        }
       
        private void SetTextBox(String text)
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        
    }
    
}

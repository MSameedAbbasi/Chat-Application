using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chatproj
{
    public partial class loginForm2 : Form
    {
        public static List<string> users = new List<string>();


        private string connection_string = "Data Source=(LocalDB)/MSSQLLocalDB;AttachDbFilename= " + '\u0022' +
            "C:/Users/Omex/Desktop/Sameed/VS programs/ChatApp Client-Server/ChatApp Client-Server/Database1.mdf " 
            + '\u0022' + ";Integrated Security=True;Connect Timeout=30";

        private string connection_string2 = "Data Source=(LocalDB)/MSSQLLocalDB;Initial Catalog=\"C:/USERS/OMEX" +
            "/DESKTOP/SAMEED/VS PROGRAMS/CHATAPP CLIENT-SERVER/CHATAPP CLIENT-SERVER/DATABASE1.MDF\";Integrated " +
            "Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=" +
            "ReadWrite;MultiSubnetFailover=False";
        public loginForm2()
        {
            InitializeComponent();
        }

        private void loginForm2_Load(object sender, EventArgs e)
        {
            Console.WriteLine(connection_string);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //tempauthen();
            //databasefunc();
        }

        private void tempauthen()
        {
            if (textBox1.Text == "sameed" && textBox2.Text == "sameed123")
            {
                client.username = "Sameed";
                users.Add(textBox1.Text);
                server_form form1 = new server_form();
                form1.Show();
                this.Hide();
                
            }
            else if (textBox1.Text == "faizan" && textBox2.Text == "faizan123")
            {
                client.username = "Faizan";
                users.Add(textBox1.Text);
                client fo = new client();
                fo.Show();
                this.Hide();
            }
            else if (textBox1.Text == "ali" && textBox2.Text == "ali123")
            {
                client.username = "Ali";
                users.Add(textBox1.Text);
                client fo = new client();
                fo.Show();
                this.Hide();
            }
            else 
            {
                
                MessageBox.Show("incorrect");
            }
        }

        private void databasefunc()
        {
            try
            {
                string sql_command = "select count(client_name) from clients where client_name=@name and client_password = @pass;";


                SqlConnection sqlconn;
                sqlconn = new SqlConnection(connection_string);
                sqlconn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand(sql_command, sqlconn);
                cmd.Parameters.AddWithValue("@name", textBox1.Text);

                cmd.Parameters.AddWithValue("@pass", textBox2.Text);

                /*SqlDataAdapter sdr = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sdr.Fill(dt);*/
                if (cmd.ExecuteScalar().ToString() == "1")
                {

                    server_form form1 = new server_form();
                    form1.Show();
                    this.Close();
                }
                else
                {
                    label1.Text = "incorrect credentials";
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("retry -- "+e);
            }
        }


        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            tempauthen();
        }
    }
}

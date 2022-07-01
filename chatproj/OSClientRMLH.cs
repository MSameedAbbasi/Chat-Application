using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;
using chatproj;

namespace XQ_Client
{
    public interface IOSClientRMLH
    {
        void osClient_onMessage(string message);
        void osClient_OnConnectionError();
        void Login();
    }
    public class OSClientRMLH
    {
        public const string MAIN_WIN_HB_TEXT = "OMEX_MAIN_HEART_BEAT";
        public bool ShowPopupsShell = false;
        bool messageBoxShown = false;
        int _NoOfConnectionAttempt = 0;
        public int NoOfConnectionAttempt
        {
            get
            {
                return _NoOfConnectionAttempt;
            }
        }
        Form form;
        string heartbeatText;
        string host;
        int port;
        bool useSSL;
        public delegate void DataReceiverDelegate();
        public event DataReceiverDelegate DataReceiverCallback;
        Form historyBlotterLoggingInstance;
        public OSClientRMLH(Form form, IOSClientRMLH callbackInstance, string IpAddress, int Port, bool UseSSL, string heartbeatText)
        {
            
            formInstance = callbackInstance;
            this.form = form;
            this.heartbeatText = heartbeatText;
            useSSL = UseSSL;
            this.port = Port;
            this.host = IpAddress;
        }

        public OSClientRMLH(Form form, Form history, IOSClientRMLH callbackInstance, string IpAddress, int Port, bool UseSSL, string heartbeatText)
        {
           

            formInstance = callbackInstance;
            this.form = form;
            this.heartbeatText = heartbeatText;
            historyBlotterLoggingInstance = (Form)history;

            useSSL = UseSSL;
            this.port = Port;
            this.host = IpAddress;
        }

        private bool isConnected;
        public bool Connected
        {
            get
            {
                return isConnected;
            }
        }

        readonly byte[] tempBuffer = new byte[1024];
        StringBuilder textBuffer;
        SslSocketStreamAsync sock;
        Thread thd;
        IOSClientRMLH formInstance;
        System.Threading.Timer heartbeatTimer;
        bool initializationInProgress = false;

        private void Connect()
        {
            if (initializationInProgress) return;
            if (isConnected) return;
            initializationInProgress = true;
            try
            {
                if (sock != null)
                    try
                    {
                        //Console.WriteLine("CLOSING.....SOCKET");
                        sock.Close();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch (Exception ex)
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                    }
                if (thd != null)
                    try
                    {
                        thd.Abort();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch (Exception ex)
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                    }
                Thread.Sleep(1000);
                var innerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // ReSharper disable AssignNullToNotNullAttribute
                _NoOfConnectionAttempt++;
                if (host != null) innerSock.Connect(IPAddress.Parse(host), port);
                // ReSharper restore AssignNullToNotNullAttribute
                innerSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                innerSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 45000);//expected timeout for heartbeats
                sock = new SslSocketStreamAsync(new SslSocketStreamAsync.SocketStream(innerSock, useSSL));
                sock.AuthenticateAsClient();
                textBuffer = new StringBuilder();
                this.DataReceiverCallback += new DataReceiverDelegate(DRCallback);
                thd = new Thread(DataReceiver) { IsBackground = true };
                thd.Start();
                isConnected = true;
                if (heartbeatTimer == null)
                {
                    heartbeatTimer = new System.Threading.Timer(OnHeartbeat, null, 30000, 30000);
                }
                this.formInstance.Login();
                initializationInProgress = false;
                socketTeardown = false;
            }
            catch (Exception ex)
            {
                //if (ShowPopupsShell)
                //{
                //    if (!messageBoxShown)
                //    {
                //        messageBoxShown = true;
                //        MessageBox.Show(form, "Could not connect to the server at " + host, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    }
                //}

                initializationInProgress = false;
                Reconnect();
                if (historyBlotterLoggingInstance != null && historyBlotterLoggingInstance.Text == "History Blotter")
                {
                }
            }
        }
        private void OnHeartbeat(object o)
        {
            write("8=EXX.1.0#35=0#52=" + DateTime.Now.ToString("HH:mm:ss-MMddyyyy") + "#58=" + heartbeatText + "#");
        }


        private void DataReceiver()
        {
            try
            {

                while (true)
                {
                    int iRx = sock.Receive(tempBuffer, 0, tempBuffer.Length, SocketFlags.None);
                    if (iRx == 0)
                    {
                        //socket closed
                        isConnected = false;
                        OnCloseInternal();
                        Reconnect();
                        if (historyBlotterLoggingInstance != null && historyBlotterLoggingInstance.Text == "History Blotter")
                        {
                        }
                        return;
                    }
                    textBuffer.Append(Encoding.ASCII.GetString(tempBuffer, 0, iRx));
                    if (textBuffer.Length >= 1 &&
                        textBuffer[textBuffer.Length - 1] == '\n')
                    {
                        try
                        {
                            if (DataReceiverCallback != null)
                                DataReceiverCallback();

                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex.ToString());
                            textBuffer = new StringBuilder();
                            OnCloseInternal();
                            isConnected = false;
                            //Console.WriteLine("CLOSING.....SOCKET3");
                            sock.Close();
                            Reconnect();
                            if (historyBlotterLoggingInstance != null && historyBlotterLoggingInstance.Text == "History Blotter")
                            {
                            }
                            return;
                        }
                        textBuffer = new StringBuilder();
                    }
                }

            }
            catch (Exception ex)
            {
                //NeXXApiDotNet.NeXXCoreBase.LogMessage(ex.ToString());
                isConnected = false;
                OnCloseInternal();
                Reconnect();
                if (historyBlotterLoggingInstance != null && historyBlotterLoggingInstance.Text == "History Blotter")
                {
                }
            }
        }

        private void DRCallback()
        {
            foreach (string str in textBuffer.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {

                try
                {
                    ReceivedMessage(str);
                }
                catch (Exception exx)
                {
                }
            }
        }


        public delegate void OnCloseDelegate();
        readonly object oneclose = new object();
        bool closeInProcess;
        private void OnCloseInternal()
        {
            lock (oneclose)
            {
                if (closeInProcess) return;
                closeInProcess = true;
            }
            //call onclose just once
            var del = new OnCloseDelegate(this.formInstance.osClient_OnConnectionError);
            del.BeginInvoke(OnCloseComplete, del);
        }
        private void OnCloseComplete(IAsyncResult ar)
        {
            try
            {
                var del = (OnCloseDelegate)ar.AsyncState;
                del.EndInvoke(ar);
                //completed
                closeInProcess = false;
            }
            catch
            {
                //log error.
            }
        }
        bool socketTeardown = false;
        /// <summary>
        /// Closes Socket and terminated the initialized thread
        /// </summary>
        public void close()
        {

            //Console.WriteLine("CONNECTION CLOSED");
            this.DataReceiverCallback -= new DataReceiverDelegate(DRCallback);
            socketTeardown = true;
            closeInProcess = true;
            if (sock != null)
            {
                try
                {
                    //Console.WriteLine("CLOSING.....SOCKET2");
                    sock.Shutdown(SocketShutdown.Both);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch (Exception ex)
                // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }
            if (thd != null)
            {
                try
                {
                    thd.Abort();
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch (Exception ex)
                // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }
            isConnected = false;
            closeInProcess = false;
        }

        /// <summary>
        /// Writes a specified string to network resource (socket) as an arg
        /// </summary>
        /// <param name="message">string message to be sent</param>
        public bool write(String message)
        {
            lock (this)
            {

                if (heartbeatText == MAIN_WIN_HB_TEXT)
                {
                }

                bool retVal = true;
                try
                {
                    message = message.Replace("\n", "");
                    message = message.Replace("\r", "");
                    message = message.Replace("\t", "");
                    byte[] sendByte = System.Text.Encoding.ASCII.GetBytes(message + "\n");
                    if (sock.Send(sendByte, 0, sendByte.Length, SocketFlags.None) == 0)
                    {
                        retVal = false;
                    }
                }
                catch (Exception e)
                {
                    retVal = false;
                    //Console.WriteLine(e);
                    isConnected = false;
                    OnCloseInternal();
                    Reconnect();
                    if (historyBlotterLoggingInstance != null && historyBlotterLoggingInstance.Text == "History Blotter")
                    {
                    }
                }
                return retVal;
            }

        }

        public bool writeWithEscapeSequence(String message)
        {
            if (heartbeatText == MAIN_WIN_HB_TEXT)
            {
            }

            bool retVal = true;
            try
            {
                byte[] sendByte = System.Text.Encoding.ASCII.GetBytes(message + "\n");
                if (sock.Send(sendByte, 0, sendByte.Length, SocketFlags.None) == 0)
                {
                    retVal = false;
                }
            }
            catch (Exception e)
            {
                retVal = false;
                isConnected = false;
                OnCloseInternal();
                Reconnect();
                if (historyBlotterLoggingInstance != null && historyBlotterLoggingInstance.Text == "History Blotter")
                {
                }
            }

            return retVal;
        }

        /// <summary>
        /// Writes a specified string to network resource (socket) as an arg
        /// </summary>
        /// <param name="message">string message to be sent</param>
        public bool WriteMany(String message)
        {
            bool retVal = true;
            try
            {
                string[] parts = message.Split('\n');
                foreach (string s in parts)
                {
                    byte[] sendByte = System.Text.Encoding.ASCII.GetBytes(s + "\n");
                    if (sock.Send(sendByte, 0, sendByte.Length, SocketFlags.None) == 0)
                    {
                        retVal = false;
                    }
                }
            }
            catch (Exception e)
            {
                retVal = false;
                isConnected = false;
                OnCloseInternal();
                Reconnect();
                if (historyBlotterLoggingInstance != null && historyBlotterLoggingInstance.Text == "History Blotter")
                {
                }
            }
            return retVal;

        }
        private void ReceivedMessage(string text)
        {
            if (heartbeatText == MAIN_WIN_HB_TEXT)
            {
            }

            if (text != null)
            {
                if (text.IndexOf("#35=0") < 0)//no need to send heartbeats to form
                {
                    formInstance.osClient_onMessage(text.ToString());
                }
            }
        }
        void Reconnect()
        {
            formInstance.osClient_OnConnectionError();
            if (!socketTeardown)
                StartInitialization();

        }
        /// <summary>
        /// A new method instead of startRead() this one creates a thread to read from socket.
        /// </summary>
        public void StartInitialization()
        {
            if (!Connected)
                Connect();
                //new OnCloseDelegate(this.Connect).BeginInvoke(null, null);
        }
    }
}

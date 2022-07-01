using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Diagnostics;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Authentication;

namespace chatproj
{
    internal class SslSocketStreamAsync: SslStream
    {
        //public static int TIMEOUT = 100000;
        /// <summary>
        /// SocketStream helps SslStream in getting its data asyncronusly.
        /// </summary>
        public class SocketStream : Stream
        {
            private byte[] _sockBuffer = new byte[1];
            public Socket Socket { get { return _socket; } }
            private Socket _socket;
            private bool _isUsingSsl;
            public bool IsUsingSsl
            {
                get
                {
                    return _isUsingSsl;
                }
            }
            public SocketStream(Socket socket, bool isUsingSsl)
            {
                _isUsingSsl = isUsingSsl;
                _socket = socket;
            }
            public bool Connected
            {
                get
                {
                    return _connected;
                }
            }
            private bool _connected = true;
            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {

            }

            public override long Length
            {
                get { throw new Exception("The method or operation is not implemented."); }
            }

            public override long Position
            {
                get
                {
                    throw new Exception("The method or operation is not implemented.");
                }
                set
                {
                    throw new Exception("The method or operation is not implemented.");
                }
            }
            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return this._socket.BeginReceive(buffer, offset, count, SocketFlags.None, callback, state);
            }
            public override int EndRead(IAsyncResult asyncResult)
            {
                try
                {
                    return this._socket.EndReceive(asyncResult);
                }
                catch (SocketException)
                {
                    _connected = false;
                    return 0;
                }
            }
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return this._socket.BeginSend(buffer, offset, count, SocketFlags.None, callback, state);
            }
            public override void EndWrite(IAsyncResult asyncResult)
            {
                try
                {
                    this._socket.EndSend(asyncResult);
                }
                catch (SocketException)
                {
                    _connected = false;
                }
            }
            public override void WriteByte(byte value)
            {
                try
                {
                    this._socket.Send(new byte[] { value }, 0, 1, SocketFlags.None);
                }
                catch (SocketException)
                {
                    _connected = false;
                }
            }


            public override int ReadByte()
            {
                try
                {
                    byte[] buffer = new byte[1];
                    this._socket.Receive(buffer, 0, 1, SocketFlags.None);
                    return buffer[0];
                }
                catch (SocketException)
                {
                    _connected = false;
                    return 0;
                }
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                try
                {
                    return this._socket.Receive(buffer, offset, count, SocketFlags.None);
                }
                catch (SocketException)
                {
                    _connected = false;
                    return 0;
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public override void SetLength(long value)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                try
                {
                    _socket.Send(buffer, offset, count, SocketFlags.None);
                }
                catch (SocketException)
                {
                    _connected = false;
                }
            }
        }
        private SocketStream _innerSocketStream;
        public SslSocketStreamAsync(Stream innerStream)
            : base(innerStream, false, new RemoteCertificateValidationCallback(CertificateValidationCallback))
        {
            this._innerSocketStream = (SocketStream)innerStream;
            _remoteEndPoint = new IPEndPoint(((IPEndPoint)_innerSocketStream.Socket.RemoteEndPoint).Address, ((IPEndPoint)_innerSocketStream.Socket.RemoteEndPoint).Port);
        }
        private static bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                return true;
            }
            else
                return true;
        }

        public override void AuthenticateAsClient(string targetHost)
        {
            base.AuthenticateAsClient(targetHost);
        }

        public override void AuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
        {
            base.AuthenticateAsClient(targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation);
        }
        public override void AuthenticateAsServer(X509Certificate serverCertificate)
        {
            base.AuthenticateAsServer(serverCertificate);
        }
        public override void AuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
        {
            base.AuthenticateAsServer(serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation);
        }


        public void AuthenticateAsClient()
        {
            if (_innerSocketStream.IsUsingSsl)
            {
                base.AuthenticateAsClient("CertName");
            }
        }
        byte[] lastBuffer;
        int lastOfset;
        int lastSize;
        public int EndReceive(IAsyncResult asyncResult)
        {
            if (_innerSocketStream.IsUsingSsl)
            {
                int stst = base.EndRead(asyncResult);
                if (stst > 0)
                {
                    //File.AppendAllText(sysSeq + "log.txt", System.Text.Encoding.ASCII.GetString(lastBuffer, lastOfset, stst));
                    //sysSeq++;
                }
                return stst;
            }
            else
            {
                return this._innerSocketStream.Socket.EndReceive(asyncResult);
            }
        }
        public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            if (_innerSocketStream.IsUsingSsl)
            {
                lastBuffer = buffer;
                lastOfset = offset;
                lastSize = size;
                return this.BeginRead(buffer, offset, size, callback, state);
            }
            else
            {
                return this._innerSocketStream.Socket.BeginReceive(buffer, offset, size, socketFlags, callback, state);

            }
        }
        public bool Connected { get { return _innerSocketStream.IsUsingSsl ? _innerSocketStream.Connected : this._innerSocketStream.Socket.Connected; } }
        public int Available { get { return _innerSocketStream.IsUsingSsl ? (_innerSocketStream.Connected ? 1 : 0) : (_innerSocketStream.Socket.Connected ? 1 : 0); } }
        public void Shutdown(SocketShutdown how)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.WriteLine("Stream::Shutdown called from: " + System.Environment.StackTrace);
                Console.ResetColor();

                this.Close();
                _innerSocketStream.Socket.Close();
            }
            catch (Exception)
            {
            }
        }
        static int sysSeq = 1;
        private IPEndPoint _remoteEndPoint;
        public EndPoint RemoteEndPoint { get { return _remoteEndPoint; } }
        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (_innerSocketStream.IsUsingSsl)

            {
                int stst = -1;
                //try
                //{
                stst = base.Read(buffer, offset, size);
                //}
                //catch (ObjectDisposedException)
                //{

                //}
                //                File.AppendAllText(sysSeq+"log.txt",System.Text.Encoding.ASCII.GetString(buffer,offset,size));
                //              sysSeq++;
                return stst;
            }
            else
            {
                return _innerSocketStream.Socket.Receive(buffer, offset, size, SocketFlags.None);
            }
        }
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            try
            {
                if (_innerSocketStream.IsUsingSsl)
                {
                    base.Write(buffer, offset, size);
                }
                else
                {
                    _innerSocketStream.Socket.Send(buffer, offset, size, SocketFlags.None);
                }
            }
            catch (SocketException)
            {
                return 0;
            }
            return size;
        }
    }
}

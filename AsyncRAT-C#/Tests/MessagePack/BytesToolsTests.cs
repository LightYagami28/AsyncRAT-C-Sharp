using System;
using System.Text;
using MessagePackLib.MessagePack;
using NUnit.Framework;

namespace AsyncRAT.Tests.MessagePack
{
    [TestFixture]
    public class BytesToolsTests
    {
        [Test]
        public void GetUtf8Bytes_AsciiString_CorrectBytes()
        {
            byte[] result = BytesTools.GetUtf8Bytes("ABC");
            Assert.That(result, Is.EqualTo(new byte[] { 0x41, 0x42, 0x43 }));
        }

        [Test]
        public void GetString_AsciiBytes_CorrectString()
        {
            string result = BytesTools.GetString(new byte[] { 0x48, 0x69 });
            Assert.That(result, Is.EqualTo("Hi"));
        }

        [Test]
        public void GetUtf8Bytes_ThenGetString_RoundTrip()
        {
            string original = "Hello, \u00e9";
            byte[] bytes = BytesTools.GetUtf8Bytes(original);
            string result = BytesTools.GetString(bytes);
            Assert.That(result, Is.EqualTo(original));
        }

        [Test]
        public void BytesAsString_FormatsDecimalPadded()
        {
            byte[] input = new byte[] { 1, 10, 255 };
            string result = BytesTools.BytesAsString(input);
            Assert.That(result, Does.Contain("001 "));
            Assert.That(result, Does.Contain("010 "));
            Assert.That(result, Does.Contain("255 "));
        }

        [Test]
        public void BytesAsHexString_FormatsUppercaseHex()
        {
            byte[] input = new byte[] { 0x0A, 0xFF, 0x10 };
            string result = BytesTools.BytesAsHexString(input);
            Assert.That(result, Does.Contain("0A "));
            Assert.That(result, Does.Contain("FF "));
            Assert.That(result, Does.Contain("10 "));
        }

        [Test]
        public void SwapBytes_ReversesByteOrder()
        {
            byte[] input = new byte[] { 1, 2, 3, 4 };
            byte[] result = BytesTools.SwapBytes(input);
            Assert.That(result, Is.EqualTo(new byte[] { 4, 3, 2, 1 }));
        }

        [Test]
        public void SwapBytes_SingleByte_ReturnsSame()
        {
            byte[] input = new byte[] { 0xAB };
            byte[] result = BytesTools.SwapBytes(input);
            Assert.That(result, Is.EqualTo(new byte[] { 0xAB }));
        }

        [Test]
        public void SwapBytes_EmptyArray_ReturnsEmpty()
        {
            byte[] result = BytesTools.SwapBytes(Array.Empty<byte>());
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public void SwapInt32_BigEndian_ByteOrder()
        {
            // 0x01020304 should become big-endian bytes [01, 02, 03, 04]
            byte[] result = BytesTools.SwapInt32(0x01020304);
            Assert.That(result, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04 }));
        }

        [Test]
        public void SwapInt32_Zero_ReturnsAllZeroBytes()
        {
            byte[] result = BytesTools.SwapInt32(0);
            Assert.That(result, Is.EqualTo(new byte[] { 0, 0, 0, 0 }));
        }

        [Test]
        public void SwapInt16_BigEndian_ByteOrder()
        {
            // 0x0102 should become [01, 02]
            byte[] result = BytesTools.SwapInt16(0x0102);
            Assert.That(result, Is.EqualTo(new byte[] { 0x01, 0x02 }));
        }

        [Test]
        public void SwapInt64_BigEndian_ByteOrder()
        {
            // 0x0102030405060708 should become [01,02,03,04,05,06,07,08]
            byte[] result = BytesTools.SwapInt64(0x0102030405060708L);
            Assert.That(result, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }));
        }

        [Test]
        public void SwapDouble_RoundTrip()
        {
            double value = 3.14159265358979;
            byte[] swapped = BytesTools.SwapDouble(value);
            // Re-swap should restore original byte layout
            byte[] original = BytesTools.SwapDouble(value);
            Assert.That(swapped, Is.EqualTo(original));
        }
    }
}

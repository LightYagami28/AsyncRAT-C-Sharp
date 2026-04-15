using System.Text;

namespace AsyncRat.Tests.Algorithm
{
    [TestFixture]
    public class ZipTests
    {
        // ── Compress / Decompress round-trip ─────────────────────────────────────

        [Test]
        public void CompressDecompress_SimpleBytes_RoundTrip()
        {
            byte[] input = Encoding.UTF8.GetBytes("Hello, GZip compression!");
            byte[] compressed = Server.Algorithm.Zip.Compress(input);
            byte[] decompressed = Server.Algorithm.Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(input));
        }

        [Test]
        public void CompressDecompress_EmptyArray_RoundTrip()
        {
            byte[] input = Array.Empty<byte>();
            byte[] compressed = Server.Algorithm.Zip.Compress(input);
            byte[] decompressed = Server.Algorithm.Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(input));
        }

        [Test]
        public void CompressDecompress_SingleByte_RoundTrip()
        {
            byte[] input = { 0x42 };
            byte[] compressed = Server.Algorithm.Zip.Compress(input);
            byte[] decompressed = Server.Algorithm.Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(input));
        }

        [Test]
        public void CompressDecompress_AllZeroBytes_RoundTrip()
        {
            byte[] input = new byte[1024]; // all zeros — highly compressible
            byte[] compressed = Server.Algorithm.Zip.Compress(input);
            byte[] decompressed = Server.Algorithm.Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(input));
        }

        [Test]
        public void CompressDecompress_BinaryData_RoundTrip()
        {
            byte[] input = new byte[256];
            for (int i = 0; i < 256; i++) input[i] = (byte)i;
            byte[] compressed = Server.Algorithm.Zip.Compress(input);
            byte[] decompressed = Server.Algorithm.Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(input));
        }

        [Test]
        public void CompressDecompress_LargeData_RoundTrip()
        {
            byte[] input = new byte[100_000];
            new Random(42).NextBytes(input);
            byte[] compressed = Server.Algorithm.Zip.Compress(input);
            byte[] decompressed = Server.Algorithm.Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(input));
        }

        // ── Compress output structure ─────────────────────────────────────────────

        [Test]
        public void Compress_Output_ContainsLengthPrefixAs4Bytes()
        {
            byte[] input = Encoding.UTF8.GetBytes("test");
            byte[] compressed = Server.Algorithm.Zip.Compress(input);

            // First 4 bytes = original length as little-endian int32
            int storedLength = BitConverter.ToInt32(compressed, 0);
            Assert.That(storedLength, Is.EqualTo(input.Length));
        }

        [Test]
        public void Compress_HighlyCompressibleInput_SmallerThanOriginal()
        {
            // 10 000 identical bytes should compress well
            byte[] input = new byte[10_000]; // all zeros
            byte[] compressed = Server.Algorithm.Zip.Compress(input);

            // Compressed size (minus 4-byte header) should be smaller
            Assert.That(compressed.Length, Is.LessThan(input.Length));
        }
    }
}

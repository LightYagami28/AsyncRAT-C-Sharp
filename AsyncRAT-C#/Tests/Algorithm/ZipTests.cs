using System;
using System.Text;
using NUnit.Framework;
using Server.Algorithm;

namespace AsyncRAT.Tests.Algorithm
{
    [TestFixture]
    public class ZipTests
    {
        [Test]
        public void CompressDecompress_SmallByteArray_RoundTrip()
        {
            byte[] original = Encoding.UTF8.GetBytes("Hello, World!");
            byte[] compressed = Zip.Compress(original);
            byte[] decompressed = Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(original));
        }

        [Test]
        public void CompressDecompress_LargeByteArray_RoundTrip()
        {
            byte[] original = new byte[65536];
            for (int i = 0; i < original.Length; i++)
                original[i] = (byte)(i % 256);
            byte[] compressed = Zip.Compress(original);
            byte[] decompressed = Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(original));
        }

        [Test]
        public void CompressDecompress_HighEntropyData_RoundTrip()
        {
            byte[] original = new byte[1024];
            new Random(99).NextBytes(original);
            byte[] compressed = Zip.Compress(original);
            byte[] decompressed = Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(original));
        }

        [Test]
        public void CompressDecompress_AllZeroBytes_RoundTrip()
        {
            byte[] original = new byte[512];
            byte[] compressed = Zip.Compress(original);
            byte[] decompressed = Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(original));
        }

        [Test]
        public void CompressDecompress_SingleByte_RoundTrip()
        {
            byte[] original = new byte[] { 0xAB };
            byte[] compressed = Zip.Compress(original);
            byte[] decompressed = Zip.Decompress(compressed);
            Assert.That(decompressed, Is.EqualTo(original));
        }

        [Test]
        public void Compress_RepetitiveData_SmallerThanOriginal()
        {
            // Highly repetitive data should compress well
            byte[] original = new byte[10000];
            // all bytes same value - very compressible
            Array.Fill(original, (byte)0xAA);
            byte[] compressed = Zip.Compress(original);
            // Subtract the 4-byte length prefix from compressed size for comparison
            Assert.That(compressed.Length, Is.LessThan(original.Length));
        }

        [Test]
        public void Compress_OutputStartsWithOriginalLength()
        {
            byte[] original = Encoding.UTF8.GetBytes("test data");
            byte[] compressed = Zip.Compress(original);

            // First 4 bytes encode the original length as Int32
            int storedLength = BitConverter.ToInt32(compressed, 0);
            Assert.That(storedLength, Is.EqualTo(original.Length));
        }

        [Test]
        public void CompressDecompress_StringData_RoundTrip()
        {
            string text = "The quick brown fox jumps over the lazy dog. " +
                          "Repeated content makes compression effective. " +
                          "The quick brown fox jumps over the lazy dog.";
            byte[] original = Encoding.UTF8.GetBytes(text);
            byte[] decompressed = Zip.Decompress(Zip.Compress(original));
            Assert.That(Encoding.UTF8.GetString(decompressed), Is.EqualTo(text));
        }

        [Test]
        public void CompressDecompress_BinaryData_RoundTrip()
        {
            // Simulate binary payload (e.g. plugin DLL-like data)
            byte[] original = new byte[2048];
            for (int i = 0; i < original.Length; i++)
                original[i] = (byte)((i * 7 + 13) % 256);
            byte[] decompressed = Zip.Decompress(Zip.Compress(original));
            Assert.That(decompressed, Is.EqualTo(original));
        }

        [Test]
        public void Compress_NonNullOutput()
        {
            byte[] result = Zip.Compress(Encoding.UTF8.GetBytes("data"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Decompress_NonNullOutput()
        {
            byte[] compressed = Zip.Compress(Encoding.UTF8.GetBytes("data"));
            byte[] result = Zip.Decompress(compressed);
            Assert.That(result, Is.Not.Null);
        }
    }
}

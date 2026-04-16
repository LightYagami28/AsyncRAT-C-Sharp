using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Server.Algorithm;

namespace AsyncRAT.Tests.Algorithm
{
    [TestFixture]
    public class GetHashTests
    {
        private string _tempFile;

        [SetUp]
        public void SetUp()
        {
            _tempFile = Path.GetTempFileName();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFile))
                File.Delete(_tempFile);
        }

        [Test]
        public void GetChecksum_KnownContent_ReturnsExpectedHash()
        {
            // Write known content to temp file
            byte[] content = Encoding.UTF8.GetBytes("Hello");
            File.WriteAllBytes(_tempFile, content);

            // SHA-256 of "Hello" without newline, formatted as uppercase hex without hyphens
            byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(content);
            string expected = hashBytes.Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2"))).ToString();

            string result = GetHash.GetChecksum(_tempFile);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GetChecksum_EmptyFile_Returns64CharString()
        {
            File.WriteAllBytes(_tempFile, System.Array.Empty<byte>());
            string result = GetHash.GetChecksum(_tempFile);
            Assert.That(result.Length, Is.EqualTo(64));
        }

        [Test]
        public void GetChecksum_SameContentSameHash()
        {
            byte[] data = Encoding.UTF8.GetBytes("same content");
            File.WriteAllBytes(_tempFile, data);
            string hash1 = GetHash.GetChecksum(_tempFile);

            string tempFile2 = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFile2, data);
                string hash2 = GetHash.GetChecksum(tempFile2);
                Assert.That(hash1, Is.EqualTo(hash2));
            }
            finally
            {
                File.Delete(tempFile2);
            }
        }

        [Test]
        public void GetChecksum_DifferentContentDifferentHash()
        {
            File.WriteAllBytes(_tempFile, Encoding.UTF8.GetBytes("content A"));

            string tempFile2 = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFile2, Encoding.UTF8.GetBytes("content B"));
                Assert.That(GetHash.GetChecksum(_tempFile), Is.Not.EqualTo(GetHash.GetChecksum(tempFile2)));
            }
            finally
            {
                File.Delete(tempFile2);
            }
        }

        [Test]
        public void GetChecksum_ReturnsUppercaseHex()
        {
            File.WriteAllBytes(_tempFile, Encoding.UTF8.GetBytes("test"));
            string result = GetHash.GetChecksum(_tempFile);
            // Verify no lowercase hex chars
            Assert.That(result, Is.EqualTo(result.ToUpper()));
        }

        [Test]
        public void GetChecksum_NoHyphens()
        {
            File.WriteAllBytes(_tempFile, Encoding.UTF8.GetBytes("no hyphens"));
            string result = GetHash.GetChecksum(_tempFile);
            Assert.That(result, Does.Not.Contain("-"));
        }
    }
}

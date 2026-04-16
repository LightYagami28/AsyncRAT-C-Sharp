using System.Text;
using NUnit.Framework;
using Server.Algorithm;

namespace AsyncRAT.Tests.Algorithm
{
    [TestFixture]
    public class ServerSha256Tests
    {
        // Known SHA-256 hash of "hello" (uppercase hex as produced by the class)
        private const string HelloHash = "2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824";
        // Known SHA-256 hash of empty string
        private const string EmptyHash = "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855";

        [Test]
        public void ComputeHash_String_KnownHash()
        {
            string result = Sha256.ComputeHash("hello");
            Assert.That(result, Is.EqualTo(HelloHash));
        }

        [Test]
        public void ComputeHash_EmptyString_KnownHash()
        {
            string result = Sha256.ComputeHash(string.Empty);
            Assert.That(result, Is.EqualTo(EmptyHash));
        }

        [Test]
        public void ComputeHash_String_ReturnsUppercase()
        {
            string result = Sha256.ComputeHash("test");
            Assert.That(result, Is.EqualTo(result.ToUpper()));
        }

        [Test]
        public void ComputeHash_String_Returns64Chars()
        {
            string result = Sha256.ComputeHash("any input");
            Assert.That(result.Length, Is.EqualTo(64));
        }

        [Test]
        public void ComputeHash_SameInput_ProducesSameOutput()
        {
            string input = "consistent input";
            Assert.That(Sha256.ComputeHash(input), Is.EqualTo(Sha256.ComputeHash(input)));
        }

        [Test]
        public void ComputeHash_DifferentInputs_ProduceDifferentHashes()
        {
            Assert.That(Sha256.ComputeHash("input1"), Is.Not.EqualTo(Sha256.ComputeHash("input2")));
        }

        [Test]
        public void ComputeHash_Bytes_KnownHash()
        {
            byte[] input = Encoding.UTF8.GetBytes("hello");
            byte[] expected = System.Security.Cryptography.SHA256.HashData(input);
            byte[] result = Sha256.ComputeHash(input);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ComputeHash_Bytes_EmptyInput_Returns32Bytes()
        {
            byte[] result = Sha256.ComputeHash(System.Array.Empty<byte>());
            Assert.That(result.Length, Is.EqualTo(32));
        }

        [Test]
        public void ComputeHash_Bytes_SameInputProducesSameOutput()
        {
            byte[] input = Encoding.UTF8.GetBytes("deterministic");
            Assert.That(Sha256.ComputeHash(input), Is.EqualTo(Sha256.ComputeHash(input)));
        }

        [Test]
        public void ComputeHash_Bytes_DifferentInputsDifferentOutput()
        {
            byte[] a = Encoding.UTF8.GetBytes("aaa");
            byte[] b = Encoding.UTF8.GetBytes("bbb");
            Assert.That(Sha256.ComputeHash(a), Is.Not.EqualTo(Sha256.ComputeHash(b)));
        }

        [Test]
        public void ComputeHash_StringAndBytesOverload_AreConsistent()
        {
            string input = "cross-overload check";
            string hashFromString = Sha256.ComputeHash(input);
            byte[] hashFromBytes = Sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert the byte-array result to the same uppercase hex format for comparison
            var sb = new StringBuilder();
            foreach (byte b in hashFromBytes)
                sb.Append(b.ToString("X2"));

            Assert.That(hashFromString, Is.EqualTo(sb.ToString()));
        }

        [Test]
        public void ComputeHash_CaseSensitiveInput()
        {
            Assert.That(Sha256.ComputeHash("Hello"), Is.Not.EqualTo(Sha256.ComputeHash("hello")));
        }

        [Test]
        public void ComputeHash_UnicodeInput_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Sha256.ComputeHash("\u4e2d\u6587\u6d4b\u8bd5"));
        }
    }
}

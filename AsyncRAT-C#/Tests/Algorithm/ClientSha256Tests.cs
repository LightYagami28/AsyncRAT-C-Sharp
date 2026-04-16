using System.Text;
using NUnit.Framework;
using Client.Algorithm;

namespace AsyncRAT.Tests.Algorithm
{
    [TestFixture]
    public class ClientSha256Tests
    {
        private const string HelloHash = "2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824";

        [Test]
        public void ComputeHash_String_KnownHash()
        {
            Assert.That(Sha256.ComputeHash("hello"), Is.EqualTo(HelloHash));
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
            Assert.That(Sha256.ComputeHash("any string").Length, Is.EqualTo(64));
        }

        [Test]
        public void ComputeHash_Bytes_Returns32Bytes()
        {
            Assert.That(Sha256.ComputeHash(Encoding.UTF8.GetBytes("test")).Length, Is.EqualTo(32));
        }

        [Test]
        public void ClientAndServerSha256_SameOutput()
        {
            string input = "consistency check";
            string serverHash = Server.Algorithm.Sha256.ComputeHash(input);
            string clientHash = Client.Algorithm.Sha256.ComputeHash(input);
            Assert.That(clientHash, Is.EqualTo(serverHash));
        }
    }
}

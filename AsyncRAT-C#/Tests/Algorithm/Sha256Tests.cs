using System.Text;

namespace AsyncRat.Tests.Algorithm
{
    [TestFixture]
    public class ServerSha256Tests
    {
        // ── String overload ───────────────────────────────────────────────────────

        [Test]
        public void ComputeHash_String_KnownValue()
        {
            // SHA-256("") = E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855
            string result = Server.Algorithm.Sha256.ComputeHash(string.Empty);
            Assert.That(result, Is.EqualTo("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855"));
        }

        [Test]
        public void ComputeHash_String_HelloWorld_KnownValue()
        {
            // SHA-256("hello") = 2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824
            string result = Server.Algorithm.Sha256.ComputeHash("hello");
            Assert.That(result, Is.EqualTo("2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824"));
        }

        [Test]
        public void ComputeHash_String_ReturnsUppercaseHex()
        {
            string result = Server.Algorithm.Sha256.ComputeHash("test");
            Assert.That(result, Is.EqualTo(result.ToUpper()));
        }

        [Test]
        public void ComputeHash_String_Returns64Characters()
        {
            string result = Server.Algorithm.Sha256.ComputeHash("any string");
            Assert.That(result.Length, Is.EqualTo(64));
        }

        [Test]
        public void ComputeHash_String_SameInput_ProducesSameOutput()
        {
            string r1 = Server.Algorithm.Sha256.ComputeHash("deterministic");
            string r2 = Server.Algorithm.Sha256.ComputeHash("deterministic");
            Assert.That(r1, Is.EqualTo(r2));
        }

        [Test]
        public void ComputeHash_String_DifferentInputs_ProduceDifferentOutputs()
        {
            string r1 = Server.Algorithm.Sha256.ComputeHash("abc");
            string r2 = Server.Algorithm.Sha256.ComputeHash("ABC");
            Assert.That(r1, Is.Not.EqualTo(r2));
        }

        // ── Byte[] overload ───────────────────────────────────────────────────────

        [Test]
        public void ComputeHash_Bytes_Returns32Bytes()
        {
            byte[] result = Server.Algorithm.Sha256.ComputeHash(new byte[] { 0x01, 0x02 });
            Assert.That(result.Length, Is.EqualTo(32));
        }

        [Test]
        public void ComputeHash_EmptyBytes_KnownValue()
        {
            // SHA-256 of empty byte array
            byte[] expected = Convert.FromHexString("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855");
            byte[] result = Server.Algorithm.Sha256.ComputeHash(Array.Empty<byte>());
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ComputeHash_Bytes_SameInput_ProducesSameOutput()
        {
            byte[] input = { 0xAA, 0xBB, 0xCC };
            byte[] r1 = Server.Algorithm.Sha256.ComputeHash(input);
            byte[] r2 = Server.Algorithm.Sha256.ComputeHash(input);
            Assert.That(r1, Is.EqualTo(r2));
        }

        [Test]
        public void ComputeHash_String_MatchesByteOverload()
        {
            const string text = "consistency check";
            string stringHash = Server.Algorithm.Sha256.ComputeHash(text);
            byte[] byteHash = Server.Algorithm.Sha256.ComputeHash(Encoding.UTF8.GetBytes(text));

            // The string overload should match manually converting byte hash to uppercase hex
            string manualHex = BitConverter.ToString(byteHash).Replace("-", "").ToUpper();
            Assert.That(stringHash, Is.EqualTo(manualHex));
        }
    }

    [TestFixture]
    public class ClientSha256Tests
    {
        [Test]
        public void ComputeHash_String_KnownValue()
        {
            string result = Client.Algorithm.Sha256.ComputeHash(string.Empty);
            Assert.That(result, Is.EqualTo("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855"));
        }

        [Test]
        public void ComputeHash_String_HelloWorld_KnownValue()
        {
            string result = Client.Algorithm.Sha256.ComputeHash("hello");
            Assert.That(result, Is.EqualTo("2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824"));
        }

        [Test]
        public void ComputeHash_String_Returns64Characters()
        {
            string result = Client.Algorithm.Sha256.ComputeHash("any string");
            Assert.That(result.Length, Is.EqualTo(64));
        }

        [Test]
        public void ComputeHash_Bytes_Returns32Bytes()
        {
            byte[] result = Client.Algorithm.Sha256.ComputeHash(new byte[] { 0x01, 0x02 });
            Assert.That(result.Length, Is.EqualTo(32));
        }

        [Test]
        public void ClientAndServer_ProduceSameHash()
        {
            const string input = "cross namespace";
            Assert.That(Client.Algorithm.Sha256.ComputeHash(input),
                        Is.EqualTo(Server.Algorithm.Sha256.ComputeHash(input)));
        }
    }
}

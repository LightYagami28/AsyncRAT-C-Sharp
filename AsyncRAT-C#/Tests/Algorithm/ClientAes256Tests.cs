using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Client.Algorithm;

namespace AsyncRAT.Tests.Algorithm
{
    [TestFixture]
    public class ClientAes256Tests
    {
        private const string ValidKey = "ClientTestKey789";
        private const string ShortMessage = "Hello from client!";

        [Test]
        public void Constructor_ValidKey_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new Aes256(ValidKey));
        }

        [Test]
        public void Constructor_NullKey_ThrowsArgumentException()
        {
            Assert.Throws<System.ArgumentException>(() => new Aes256(null!));
        }

        [Test]
        public void Constructor_EmptyKey_ThrowsArgumentException()
        {
            Assert.Throws<System.ArgumentException>(() => new Aes256(string.Empty));
        }

        [Test]
        public void EncryptDecrypt_String_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            string encrypted = aes.Encrypt(ShortMessage);
            string decrypted = aes.Decrypt(encrypted);
            Assert.That(decrypted, Is.EqualTo(ShortMessage));
        }

        [Test]
        public void EncryptDecrypt_Bytes_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            byte[] original = Encoding.UTF8.GetBytes(ShortMessage);
            byte[] encrypted = aes.Encrypt(original);
            byte[] decrypted = aes.Decrypt(encrypted);
            Assert.That(decrypted, Is.EqualTo(original));
        }

        [Test]
        public void Encrypt_NullBytes_ThrowsArgumentNullException()
        {
            var aes = new Aes256(ValidKey);
            Assert.Throws<System.ArgumentNullException>(() => aes.Encrypt((byte[])null!));
        }

        [Test]
        public void Decrypt_NullBytes_ThrowsArgumentNullException()
        {
            var aes = new Aes256(ValidKey);
            Assert.Throws<System.ArgumentNullException>(() => aes.Decrypt((byte[])null!));
        }

        [Test]
        public void Decrypt_TamperedData_ThrowsCryptographicException()
        {
            var aes = new Aes256(ValidKey);
            byte[] encrypted = aes.Encrypt(Encoding.UTF8.GetBytes(ShortMessage));
            encrypted[0] ^= 0xFF;
            Assert.Throws<CryptographicException>(() => aes.Decrypt(encrypted));
        }

        [Test]
        public void ClientAndServerAes_SameKey_InteropCompatible()
        {
            // Client and Server use identical AES-256 implementations and the same salt,
            // so encrypting on one side must be decryptable on the other with the same key.
            string key = "SharedKey!";
            var serverAes = new Server.Algorithm.Aes256(key);
            var clientAes = new Client.Algorithm.Aes256(key);

            string plaintext = "cross-side test";
            string serverEncrypted = serverAes.Encrypt(plaintext);
            string clientDecrypted = clientAes.Decrypt(serverEncrypted);
            Assert.That(clientDecrypted, Is.EqualTo(plaintext));

            string clientEncrypted = clientAes.Encrypt(plaintext);
            string serverDecrypted = serverAes.Decrypt(clientEncrypted);
            Assert.That(serverDecrypted, Is.EqualTo(plaintext));
        }

        [Test]
        public void EncryptDecrypt_EmptyBytes_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            byte[] original = System.Array.Empty<byte>();
            byte[] decrypted = aes.Decrypt(aes.Encrypt(original));
            Assert.That(decrypted, Is.EqualTo(original));
        }
    }
}

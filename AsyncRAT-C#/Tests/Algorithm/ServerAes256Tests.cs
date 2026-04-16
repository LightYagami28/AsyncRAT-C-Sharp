using System;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Server.Algorithm;

namespace AsyncRAT.Tests.Algorithm
{
    [TestFixture]
    public class ServerAes256Tests
    {
        private const string ValidKey = "TestMasterKey123";
        private const string ShortMessage = "Hello, World!";
        private const string LongMessage = "This is a longer message used to test AES-256 encryption and decryption with HMAC authentication in the AsyncRAT server algorithm.";
        private const string UnicodeMessage = "Unicode: \u00e9\u00e0\u00fc\u00f1\u4e2d\u6587";

        [Test]
        public void Constructor_ValidKey_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new Aes256(ValidKey));
        }

        [Test]
        public void Constructor_NullKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Aes256(null!));
        }

        [Test]
        public void Constructor_EmptyKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Aes256(string.Empty));
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
        public void EncryptDecrypt_LongString_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            string encrypted = aes.Encrypt(LongMessage);
            string decrypted = aes.Decrypt(encrypted);
            Assert.That(decrypted, Is.EqualTo(LongMessage));
        }

        [Test]
        public void EncryptDecrypt_UnicodeString_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            string encrypted = aes.Encrypt(UnicodeMessage);
            string decrypted = aes.Decrypt(encrypted);
            Assert.That(decrypted, Is.EqualTo(UnicodeMessage));
        }

        [Test]
        public void EncryptDecrypt_EmptyString_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            string encrypted = aes.Encrypt(string.Empty);
            string decrypted = aes.Decrypt(encrypted);
            Assert.That(decrypted, Is.EqualTo(string.Empty));
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
        public void EncryptDecrypt_LargeByteArray_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            byte[] original = new byte[4096];
            new Random(42).NextBytes(original);
            byte[] encrypted = aes.Encrypt(original);
            byte[] decrypted = aes.Decrypt(encrypted);
            Assert.That(decrypted, Is.EqualTo(original));
        }

        [Test]
        public void EncryptDecrypt_SingleByte_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            byte[] original = new byte[] { 0x42 };
            byte[] encrypted = aes.Encrypt(original);
            byte[] decrypted = aes.Decrypt(encrypted);
            Assert.That(decrypted, Is.EqualTo(original));
        }

        [Test]
        public void Encrypt_NullBytes_ThrowsArgumentNullException()
        {
            var aes = new Aes256(ValidKey);
            Assert.Throws<ArgumentNullException>(() => aes.Encrypt((byte[])null!));
        }

        [Test]
        public void Decrypt_NullBytes_ThrowsArgumentNullException()
        {
            var aes = new Aes256(ValidKey);
            Assert.Throws<ArgumentNullException>(() => aes.Decrypt((byte[])null!));
        }

        [Test]
        public void Encrypt_ProducesBase64String()
        {
            var aes = new Aes256(ValidKey);
            string encrypted = aes.Encrypt(ShortMessage);
            Assert.DoesNotThrow(() => Convert.FromBase64String(encrypted));
        }

        [Test]
        public void Encrypt_TwiceSameInput_ProducesDifferentCiphertext()
        {
            // Each encryption uses a random IV, so ciphertexts should differ
            var aes = new Aes256(ValidKey);
            string encrypted1 = aes.Encrypt(ShortMessage);
            string encrypted2 = aes.Encrypt(ShortMessage);
            Assert.That(encrypted1, Is.Not.EqualTo(encrypted2));
        }

        [Test]
        public void Decrypt_TamperedHmac_ThrowsCryptographicException()
        {
            var aes = new Aes256(ValidKey);
            byte[] encrypted = aes.Encrypt(Encoding.UTF8.GetBytes(ShortMessage));

            // Flip a byte in the HMAC region (first 32 bytes)
            encrypted[0] ^= 0xFF;

            Assert.Throws<CryptographicException>(() => aes.Decrypt(encrypted));
        }

        [Test]
        public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
        {
            var aes = new Aes256(ValidKey);
            byte[] encrypted = aes.Encrypt(Encoding.UTF8.GetBytes(ShortMessage));

            // Flip a byte in the ciphertext region (after HMAC 32 bytes + IV 16 bytes)
            encrypted[50] ^= 0xFF;

            Assert.Throws<CryptographicException>(() => aes.Decrypt(encrypted));
        }

        [Test]
        public void Decrypt_WrongKey_ThrowsCryptographicException()
        {
            var aes1 = new Aes256(ValidKey);
            var aes2 = new Aes256("DifferentKey456");

            byte[] encrypted = aes1.Encrypt(Encoding.UTF8.GetBytes(ShortMessage));

            Assert.Throws<CryptographicException>(() => aes2.Decrypt(encrypted));
        }

        [Test]
        public void Encrypt_EncryptedLengthIncludesHmacAndIv()
        {
            // Minimum encrypted length: 32 (HMAC) + 16 (IV) + 16 (one AES block)
            var aes = new Aes256(ValidKey);
            byte[] encrypted = aes.Encrypt(new byte[] { 0x01 });
            Assert.That(encrypted.Length, Is.GreaterThanOrEqualTo(64));
        }

        [Test]
        public void EncryptDecrypt_DifferentInstances_SameKey()
        {
            var aes1 = new Aes256(ValidKey);
            var aes2 = new Aes256(ValidKey);

            string encrypted = aes1.Encrypt(ShortMessage);
            string decrypted = aes2.Decrypt(encrypted);

            Assert.That(decrypted, Is.EqualTo(ShortMessage));
        }

        [Test]
        public void EncryptDecrypt_SpecialCharacters_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            string message = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~\r\n\t";
            string decrypted = aes.Decrypt(aes.Encrypt(message));
            Assert.That(decrypted, Is.EqualTo(message));
        }

        [Test]
        public void EncryptDecrypt_AllZeroBytes_RoundTrip()
        {
            var aes = new Aes256(ValidKey);
            byte[] original = new byte[32];
            byte[] decrypted = aes.Decrypt(aes.Encrypt(original));
            Assert.That(decrypted, Is.EqualTo(original));
        }
    }
}

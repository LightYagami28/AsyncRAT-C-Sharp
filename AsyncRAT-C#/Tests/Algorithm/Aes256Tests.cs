using System;
using System.Security.Cryptography;

namespace AsyncRat.Tests.Algorithm
{
    [TestFixture]
    public class ServerAes256Tests
    {
        private const string MasterKey = "TestMasterKey123";

        // ── Constructor ──────────────────────────────────────────────────────────

        [Test]
        public void Constructor_NullKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Server.Algorithm.Aes256(null!));
        }

        [Test]
        public void Constructor_EmptyKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Server.Algorithm.Aes256(string.Empty));
        }

        [Test]
        public void Constructor_ValidKey_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new Server.Algorithm.Aes256(MasterKey));
        }

        // ── Encrypt / Decrypt (string overloads) ─────────────────────────────────

        [Test]
        public void EncryptDecrypt_String_RoundTrip()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            const string plaintext = "Hello, World!";

            string ciphertext = aes.Encrypt(plaintext);
            string decrypted = aes.Decrypt(ciphertext);

            Assert.That(decrypted, Is.EqualTo(plaintext));
        }

        [Test]
        public void EncryptDecrypt_EmptyString_RoundTrip()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            string ciphertext = aes.Encrypt(string.Empty);
            string decrypted = aes.Decrypt(ciphertext);

            Assert.That(decrypted, Is.EqualTo(string.Empty));
        }

        [Test]
        public void EncryptDecrypt_UnicodeString_RoundTrip()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            const string plaintext = "日本語テスト 🎉";

            string ciphertext = aes.Encrypt(plaintext);
            string decrypted = aes.Decrypt(ciphertext);

            Assert.That(decrypted, Is.EqualTo(plaintext));
        }

        [Test]
        public void EncryptDecrypt_LongString_RoundTrip()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            string plaintext = new string('A', 10_000);

            string ciphertext = aes.Encrypt(plaintext);
            string decrypted = aes.Decrypt(ciphertext);

            Assert.That(decrypted, Is.EqualTo(plaintext));
        }

        // ── Encrypt / Decrypt (byte[] overloads) ─────────────────────────────────

        [Test]
        public void EncryptDecrypt_Bytes_RoundTrip()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            byte[] input = { 0x00, 0x01, 0x02, 0xFF, 0xFE };

            byte[] ciphertext = aes.Encrypt(input);
            byte[] decrypted = aes.Decrypt(ciphertext);

            Assert.That(decrypted, Is.EqualTo(input));
        }

        [Test]
        public void EncryptDecrypt_EmptyBytes_RoundTrip()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            byte[] input = Array.Empty<byte>();

            byte[] ciphertext = aes.Encrypt(input);
            byte[] decrypted = aes.Decrypt(ciphertext);

            Assert.That(decrypted, Is.EqualTo(input));
        }

        [Test]
        public void Encrypt_NullBytes_ThrowsArgumentNullException()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            Assert.Throws<ArgumentNullException>(() => aes.Encrypt((byte[])null!));
        }

        [Test]
        public void Decrypt_NullBytes_ThrowsArgumentNullException()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            Assert.Throws<ArgumentNullException>(() => aes.Decrypt((byte[])null!));
        }

        // ── Cross-key isolation ───────────────────────────────────────────────────

        [Test]
        public void Encrypt_DifferentKeys_ProduceDifferentCiphertexts()
        {
            var aes1 = new Server.Algorithm.Aes256("Key1");
            var aes2 = new Server.Algorithm.Aes256("Key2");
            const string plaintext = "secret data";

            string c1 = aes1.Encrypt(plaintext);
            string c2 = aes2.Encrypt(plaintext);

            Assert.That(c1, Is.Not.EqualTo(c2));
        }

        [Test]
        public void Decrypt_WrongKey_ThrowsCryptographicException()
        {
            var encryptor = new Server.Algorithm.Aes256("CorrectKey");
            var decryptor = new Server.Algorithm.Aes256("WrongKey");

            string ciphertext = encryptor.Encrypt("secret");

            Assert.Throws<CryptographicException>(() => decryptor.Decrypt(ciphertext));
        }

        // ── MAC integrity ─────────────────────────────────────────────────────────

        [Test]
        public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            byte[] ciphertext = aes.Encrypt(new byte[] { 1, 2, 3 });

            // Flip a byte inside the ciphertext region (after the 32-byte MAC + 16-byte IV)
            ciphertext[50] ^= 0xFF;

            Assert.Throws<CryptographicException>(() => aes.Decrypt(ciphertext));
        }

        [Test]
        public void Decrypt_TamperedMac_ThrowsCryptographicException()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            byte[] ciphertext = aes.Encrypt(new byte[] { 1, 2, 3 });

            // Corrupt the first byte of the HMAC header
            ciphertext[0] ^= 0xFF;

            Assert.Throws<CryptographicException>(() => aes.Decrypt(ciphertext));
        }

        // ── Non-determinism (each call produces a fresh IV) ───────────────────────

        [Test]
        public void Encrypt_SamePlaintext_ProducesDifferentCiphertexts()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            const string plaintext = "same message";

            string c1 = aes.Encrypt(plaintext);
            string c2 = aes.Encrypt(plaintext);

            // Probabilistically different due to random IV
            Assert.That(c1, Is.Not.EqualTo(c2));
        }

        // ── Output format (minimum size check) ───────────────────────────────────

        [Test]
        public void Encrypt_OutputIsAtLeast48Bytes_HmacPlusIv()
        {
            var aes = new Server.Algorithm.Aes256(MasterKey);
            byte[] ciphertext = aes.Encrypt(Array.Empty<byte>());

            // 32 HMAC + 16 IV + ≥16 ciphertext block
            Assert.That(ciphertext.Length, Is.GreaterThanOrEqualTo(48));
        }
    }

    [TestFixture]
    public class ClientAes256Tests
    {
        private const string MasterKey = "TestMasterKey123";

        [Test]
        public void Constructor_NullKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Client.Algorithm.Aes256(null!));
        }

        [Test]
        public void Constructor_EmptyKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Client.Algorithm.Aes256(string.Empty));
        }

        [Test]
        public void EncryptDecrypt_String_RoundTrip()
        {
            var aes = new Client.Algorithm.Aes256(MasterKey);
            const string plaintext = "Hello, World!";

            string ciphertext = aes.Encrypt(plaintext);
            string decrypted = aes.Decrypt(ciphertext);

            Assert.That(decrypted, Is.EqualTo(plaintext));
        }

        [Test]
        public void EncryptDecrypt_EmptyString_RoundTrip()
        {
            var aes = new Client.Algorithm.Aes256(MasterKey);
            string ciphertext = aes.Encrypt(string.Empty);
            string decrypted = aes.Decrypt(ciphertext);

            Assert.That(decrypted, Is.EqualTo(string.Empty));
        }

        [Test]
        public void EncryptDecrypt_Bytes_RoundTrip()
        {
            var aes = new Client.Algorithm.Aes256(MasterKey);
            byte[] input = { 0xDE, 0xAD, 0xBE, 0xEF };

            byte[] ciphertext = aes.Encrypt(input);
            byte[] decrypted = aes.Decrypt(ciphertext);

            Assert.That(decrypted, Is.EqualTo(input));
        }

        [Test]
        public void Encrypt_NullBytes_ThrowsArgumentNullException()
        {
            var aes = new Client.Algorithm.Aes256(MasterKey);
            Assert.Throws<ArgumentNullException>(() => aes.Encrypt((byte[])null!));
        }

        [Test]
        public void Decrypt_NullBytes_ThrowsArgumentNullException()
        {
            var aes = new Client.Algorithm.Aes256(MasterKey);
            Assert.Throws<ArgumentNullException>(() => aes.Decrypt((byte[])null!));
        }

        [Test]
        public void Decrypt_WrongKey_ThrowsCryptographicException()
        {
            var encryptor = new Client.Algorithm.Aes256("CorrectKey");
            var decryptor = new Client.Algorithm.Aes256("WrongKey");

            string ciphertext = encryptor.Encrypt("secret");

            Assert.Throws<CryptographicException>(() => decryptor.Decrypt(ciphertext));
        }

        [Test]
        public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
        {
            var aes = new Client.Algorithm.Aes256(MasterKey);
            byte[] ciphertext = aes.Encrypt(new byte[] { 1, 2, 3 });
            ciphertext[50] ^= 0xFF;

            Assert.Throws<CryptographicException>(() => aes.Decrypt(ciphertext));
        }

        // ── Cross-assembly compatibility ──────────────────────────────────────────

        [Test]
        public void ClientEncrypt_ServerDecrypt_SameKey_RoundTrip()
        {
            const string key = "SharedKey!";
            var client = new Client.Algorithm.Aes256(key);
            var server = new Server.Algorithm.Aes256(key);

            const string plaintext = "cross-assembly message";

            byte[] ciphertext = client.Encrypt(System.Text.Encoding.UTF8.GetBytes(plaintext));
            byte[] decrypted = server.Decrypt(ciphertext);

            Assert.That(System.Text.Encoding.UTF8.GetString(decrypted), Is.EqualTo(plaintext));
        }
    }
}

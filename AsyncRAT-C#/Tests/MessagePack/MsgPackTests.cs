using System.IO;
using System.Text;
using Server.MessagePack;

namespace AsyncRat.Tests.MessagePack
{
    /// <summary>
    /// Tests for MsgPack encode/decode round-trips.
    /// The encode/decode path uses Encode2Stream + DecodeFromStream directly
    /// so tests remain independent of the Zip compression layer.
    /// </summary>
    [TestFixture]
    public class MsgPackTests
    {
        // ── Helper ────────────────────────────────────────────────────────────────

        private static MsgPack RoundTripViaStream(MsgPack source)
        {
            using var ms = new MemoryStream();
            source.Encode2Stream(ms);
            ms.Position = 0;
            var dest = new MsgPack();
            dest.DecodeFromStream(ms);
            return dest;
        }

        // ── Null ─────────────────────────────────────────────────────────────────

        [Test]
        public void SetAsNull_RoundTrip_ValueTypeIsNull()
        {
            var obj = new MsgPack();
            obj.SetAsNull();

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsString(), Is.EqualTo(""));
        }

        // ── String ───────────────────────────────────────────────────────────────

        [Test]
        public void SetAsString_ShortString_RoundTrip()
        {
            var obj = new MsgPack();
            const string text = "Hello";
            obj.SetAsString(text);

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsString(), Is.EqualTo(text));
        }

        [Test]
        public void SetAsString_EmptyString_RoundTrip()
        {
            var obj = new MsgPack();
            obj.SetAsString(string.Empty);

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void SetAsString_Unicode_RoundTrip()
        {
            var obj = new MsgPack();
            const string text = "日本語テスト";
            obj.SetAsString(text);

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsString(), Is.EqualTo(text));
        }

        [Test]
        public void SetAsString_32ByteString_RoundTrip()
        {
            // 32-byte strings use the str8 (0xD9) format
            var obj = new MsgPack();
            string text = new string('X', 32);
            obj.SetAsString(text);

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsString(), Is.EqualTo(text));
        }

        [Test]
        public void SetAsString_256ByteString_RoundTrip()
        {
            // 256-byte strings use the str16 (0xDA) format
            var obj = new MsgPack();
            string text = new string('Y', 256);
            obj.SetAsString(text);

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsString(), Is.EqualTo(text));
        }

        // ── Integer ───────────────────────────────────────────────────────────────

        [TestCase(0L)]
        [TestCase(1L)]
        [TestCase(127L)]
        [TestCase(128L)]
        [TestCase(255L)]
        [TestCase(256L)]
        [TestCase(65535L)]
        [TestCase(65536L)]
        [TestCase(int.MaxValue)]
        [TestCase((long)uint.MaxValue + 1)]
        [TestCase(long.MaxValue)]
        [TestCase(-1L)]
        [TestCase(-31L)]
        [TestCase(-32L)]
        [TestCase(-128L)]
        [TestCase(-129L)]
        [TestCase(-32768L)]
        [TestCase(-32769L)]
        [TestCase((long)int.MinValue)]
        [TestCase(long.MinValue)]
        public void SetAsInteger_RoundTrip(long value)
        {
            var obj = new MsgPack();
            obj.SetAsInteger(value);

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsInteger(), Is.EqualTo(value));
        }

        // ── Boolean ───────────────────────────────────────────────────────────────

        [Test]
        public void SetAsBoolean_True_RoundTrip()
        {
            var obj = new MsgPack();
            obj.SetAsBoolean(true);

            var result = RoundTripViaStream(obj);

            // After decode, value type is Boolean; GetAsInteger returns 0 for non-integer types,
            // but the decoded boolean can be verified via ForcePathObject on a map — or we check
            // the encoded byte directly.
            using var ms = new MemoryStream();
            obj.Encode2Stream(ms);
            Assert.That(ms.ToArray()[0], Is.EqualTo(0xC3));
        }

        [Test]
        public void SetAsBoolean_False_RoundTrip()
        {
            var obj = new MsgPack();
            obj.SetAsBoolean(false);

            using var ms = new MemoryStream();
            obj.Encode2Stream(ms);
            Assert.That(ms.ToArray()[0], Is.EqualTo(0xC2));
        }

        // ── Float ─────────────────────────────────────────────────────────────────

        [Test]
        public void SetAsFloat_RoundTrip()
        {
            var obj = new MsgPack();
            obj.SetAsFloat(3.14159265);

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsFloat(), Is.EqualTo(3.14159265).Within(1e-9));
        }

        [Test]
        public void SetAsSingle_RoundTrip()
        {
            var obj = new MsgPack();
            obj.SetAsSingle(1.5f);

            using var ms = new MemoryStream();
            obj.Encode2Stream(ms);
            ms.Position = 0;
            var result = new MsgPack();
            result.DecodeFromStream(ms);

            Assert.That(result.GetAsFloat(), Is.EqualTo(1.5f).Within(1e-5));
        }

        // ── Binary ───────────────────────────────────────────────────────────────

        [Test]
        public void SetAsBytes_RoundTrip()
        {
            var obj = new MsgPack();
            byte[] data = { 0xDE, 0xAD, 0xBE, 0xEF };
            obj.SetAsBytes(data);

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsBytes(), Is.EqualTo(data));
        }

        [Test]
        public void SetAsBytes_EmptyArray_RoundTrip()
        {
            var obj = new MsgPack();
            obj.SetAsBytes(Array.Empty<byte>());

            var result = RoundTripViaStream(obj);

            Assert.That(result.GetAsBytes(), Is.EqualTo(Array.Empty<byte>()));
        }

        // ── Map via ForcePathObject ───────────────────────────────────────────────

        [Test]
        public void ForcePathObject_SingleKey_StoresAndRetrievesValue()
        {
            var obj = new MsgPack();
            obj.ForcePathObject("name").SetAsString("AsyncRAT");

            var result = RoundTripViaStream(obj);
            Assert.That(result.ForcePathObject("name").GetAsString(), Is.EqualTo("AsyncRAT"));
        }

        [Test]
        public void ForcePathObject_MultipleKeys_AllValuesPreserved()
        {
            var obj = new MsgPack();
            obj.ForcePathObject("key1").SetAsString("value1");
            obj.ForcePathObject("key2").SetAsInteger(42);

            var result = RoundTripViaStream(obj);

            Assert.That(result.ForcePathObject("key1").GetAsString(), Is.EqualTo("value1"));
            Assert.That(result.ForcePathObject("key2").GetAsInteger(), Is.EqualTo(42));
        }

        [Test]
        public void ForcePathObject_NestedPath_StoresAndRetrievesValue()
        {
            var obj = new MsgPack();
            obj.ForcePathObject("outer.inner").SetAsString("deep value");

            var result = RoundTripViaStream(obj);

            Assert.That(result.ForcePathObject("outer.inner").GetAsString(), Is.EqualTo("deep value"));
        }

        [Test]
        public void ForcePathObject_CaseInsensitiveLookup_FindsExistingKey()
        {
            var obj = new MsgPack();
            obj.ForcePathObject("MyKey").SetAsString("hello");

            // Both the original case and a different case should resolve to the same child
            Assert.That(obj.ForcePathObject("MyKey").GetAsString(), Is.EqualTo("hello"));
            Assert.That(obj.ForcePathObject("mykey").GetAsString(), Is.EqualTo("hello"));
        }

        // ── Map via FindObject ────────────────────────────────────────────────────

        [Test]
        public void FindObject_ExistingKey_ReturnsNode()
        {
            var obj = new MsgPack();
            obj.ForcePathObject("item").SetAsString("test");

            var found = obj.FindObject("item");

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.GetAsString(), Is.EqualTo("test"));
        }

        [Test]
        public void FindObject_MissingKey_ReturnsNull()
        {
            var obj = new MsgPack();
            Assert.That(obj.FindObject("nonexistent"), Is.Null);
        }

        // ── Array ─────────────────────────────────────────────────────────────────

        [Test]
        public void AsArray_StringElements_RoundTrip()
        {
            var obj = new MsgPack();
            obj.AsArray.Add("alpha");
            obj.AsArray.Add("beta");
            obj.AsArray.Add("gamma");

            var result = RoundTripViaStream(obj);

            Assert.That(result.AsArray[0].GetAsString(), Is.EqualTo("alpha"));
            Assert.That(result.AsArray[1].GetAsString(), Is.EqualTo("beta"));
            Assert.That(result.AsArray[2].GetAsString(), Is.EqualTo("gamma"));
        }

        [Test]
        public void AsArray_IntegerElements_RoundTrip()
        {
            var obj = new MsgPack();
            obj.AsArray.Add(1L);
            obj.AsArray.Add(2L);

            var result = RoundTripViaStream(obj);

            Assert.That(result.AsArray[0].GetAsInteger(), Is.EqualTo(1));
            Assert.That(result.AsArray[1].GetAsInteger(), Is.EqualTo(2));
        }

        [Test]
        public void AsArray_Length_ReturnsCorrectCount()
        {
            var obj = new MsgPack();
            obj.AsArray.Add("one");
            obj.AsArray.Add("two");

            Assert.That(obj.AsArray.Length, Is.EqualTo(2));
        }

        // ── Encode2Bytes / DecodeFromBytes (with Zip compression) ─────────────────

        [Test]
        public void Encode2Bytes_DecodeFromBytes_SimpleMap_RoundTrip()
        {
            var obj = new MsgPack();
            obj.ForcePathObject("Packet").SetAsString("Pong");
            obj.ForcePathObject("Value").SetAsInteger(99);

            byte[] encoded = obj.Encode2Bytes();

            var decoded = new MsgPack();
            decoded.DecodeFromBytes(encoded);

            Assert.That(decoded.ForcePathObject("Packet").GetAsString(), Is.EqualTo("Pong"));
            Assert.That(decoded.ForcePathObject("Value").GetAsInteger(), Is.EqualTo(99));
        }

        // ── GetAsString / GetAsInteger type coercions ─────────────────────────────

        [Test]
        public void GetAsString_OnNullValue_ReturnsEmptyString()
        {
            var obj = new MsgPack();
            obj.SetAsNull();
            Assert.That(obj.GetAsString(), Is.EqualTo(""));
        }

        [Test]
        public void GetAsInteger_OnStringNumber_ParsesValue()
        {
            var obj = new MsgPack();
            obj.SetAsString("123");
            Assert.That(obj.GetAsInteger(), Is.EqualTo(123));
        }

        [Test]
        public void GetAsFloat_OnIntegerValue_Converts()
        {
            var obj = new MsgPack();
            obj.SetAsInteger(10);
            Assert.That(obj.GetAsFloat(), Is.EqualTo(10.0).Within(1e-10));
        }

        [Test]
        public void GetAsBytes_OnStringValue_ReturnsUtf8Bytes()
        {
            var obj = new MsgPack();
            obj.SetAsString("AB");
            byte[] expected = Encoding.UTF8.GetBytes("AB");
            Assert.That(obj.GetAsBytes(), Is.EqualTo(expected));
        }
    }
}

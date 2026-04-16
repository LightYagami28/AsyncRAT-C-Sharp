using NUnit.Framework;
using Client.Helper;

namespace AsyncRAT.Tests.Helper
{
    [TestFixture]
    public class HwidGenTests
    {
        [Test]
        public void GetHash_NonEmptyInput_Returns20CharString()
        {
            string result = HwidGen.GetHash("some input data");
            Assert.That(result.Length, Is.EqualTo(20));
        }

        [Test]
        public void GetHash_ReturnsUppercaseString()
        {
            string result = HwidGen.GetHash("test");
            Assert.That(result, Is.EqualTo(result.ToUpper()));
        }

        [Test]
        public void GetHash_SameInput_ProducesSameOutput()
        {
            string input = "deterministic input";
            Assert.That(HwidGen.GetHash(input), Is.EqualTo(HwidGen.GetHash(input)));
        }

        [Test]
        public void GetHash_DifferentInputs_ProduceDifferentOutputs()
        {
            Assert.That(HwidGen.GetHash("input1"), Is.Not.EqualTo(HwidGen.GetHash("input2")));
        }

        [Test]
        public void GetHash_EmptyString_Returns20Chars()
        {
            string result = HwidGen.GetHash(string.Empty);
            Assert.That(result.Length, Is.EqualTo(20));
        }

        [Test]
        public void GetHash_LongInput_Returns20Chars()
        {
            string longInput = new string('x', 10000);
            string result = HwidGen.GetHash(longInput);
            Assert.That(result.Length, Is.EqualTo(20));
        }

        [Test]
        public void GetHash_KnownValue_MatchesMd5Prefix()
        {
            // MD5("abc") = "900150983cd24fb0d6963f7d28e17f72"
            // Uppercase first 20 chars = "900150983CD24FB0D696"
            string result = HwidGen.GetHash("abc");
            Assert.That(result, Is.EqualTo("900150983CD24FB0D696"));
        }
    }
}

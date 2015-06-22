using NUnit.Framework;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;

namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    [TestFixture]
    public class TemplateParserTests
    {
        [Test]
        public void ParsesFullPathNoExtensionOrSequence()
        {
            const string FullPath = @"C:\logs\applog";
            var result = FileNameParser.ParseLogFileName(FullPath);
            Assert.That(result.Name, Is.EqualTo("applog"));
            Assert.That(result.Sequence, Is.EqualTo(0u));
            Assert.That(result.Extension, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ParsesPartialPathNoExtensionOrSequence()
        {
            const string PartialPath = @"logs\applog";
            var result = FileNameParser.ParseLogFileName(PartialPath);
            Assert.That(result.Name, Is.EqualTo("applog"));
            Assert.That(result.Sequence, Is.EqualTo(0u));
            Assert.That(result.Extension, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ParsesFullPathWithExtensionNoSequence()
        {
            const string FullPath = @"C:\logs\applog.txt";
            var result = FileNameParser.ParseLogFileName(FullPath);
            Assert.That(result.Name, Is.EqualTo("applog"));
            Assert.That(result.Sequence, Is.EqualTo(0u));
            Assert.That(result.Extension, Is.EqualTo("txt"));
        }

        [Test]
        public void ParsesPartialPathWithExtensionNoSequence()
        {
            const string PartialPath = @"logs\applog.txt";
            var result = FileNameParser.ParseLogFileName(PartialPath);
            Assert.That(result.Name, Is.EqualTo("applog"));
            Assert.That(result.Sequence, Is.EqualTo(0u));
            Assert.That(result.Extension, Is.EqualTo("txt"));
        }

        [Test]
        public void ParsesFullPathWithExtensionAndSequence()
        {
            const string FullPath = @"C:\logs\applog00000.txt";
            var result = FileNameParser.ParseLogFileName(FullPath);
            Assert.That(result.Name, Is.EqualTo("applog"));
            Assert.That(result.Sequence, Is.EqualTo(0u));
            Assert.That(result.Extension, Is.EqualTo("txt"));
        }

        [Test]
        public void ParsesPartialPathWithExtensionAndSequence()
        {
            const string PartialPath = @"logs\applog00000.txt";
            var result = FileNameParser.ParseLogFileName(PartialPath);
            Assert.That(result.Name, Is.EqualTo("applog"));
            Assert.That(result.Sequence, Is.EqualTo(0u));
            Assert.That(result.Extension, Is.EqualTo("txt"));
        }

        [Test]
        public void ParsesFullPathWithReplacementToken()
        {
            const string FullPath = @"C:\logs\applog{Date}.txt";
            var result = FileNameParser.ParseLogFileName(FullPath);
            Assert.That(result.Name, Is.EqualTo("applog{Date}"));
            Assert.That(result.Sequence, Is.EqualTo(0u));
            Assert.That(result.Extension, Is.EqualTo("txt"));
        }
    }
}
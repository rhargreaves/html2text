using System;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using NUnit.Framework;

namespace HtmlToText.Tests
{
    [TestFixture]
    public class TextExtractorTest
    {
        [SetUp]
        public void SetUp()
        {
            _textExtractor = new TextExtractor();
            _textBuilder = new StringBuilder();
            _stringWriter = new StringWriter(_textBuilder);
        }

        private TextExtractor _textExtractor;
        private StringBuilder _textBuilder;
        private StringWriter _stringWriter;

        private HtmlDocument GetHtmlDocument(string htmlText)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlText);
            return doc;
        }

        [Test]
        public void ExtractTextAndWrite_converts_br_to_newline()
        {
            // Arrange
            HtmlNodeCollection html = GetHtmlDocument("text<br>text").DocumentNode.ChildNodes;

            // Act
            _textExtractor.ExtractTextAndWrite(html, _stringWriter);

            // Assert
            Assert.AreEqual("text" + Environment.NewLine + "text", _textBuilder.ToString());
        }

        [Test]
        [TestCase("p")]
        [TestCase("h1")]
        [TestCase("h2")]
        [TestCase("h3")]
        [TestCase("h4")]
        [TestCase("section")]
        public void ExtractTextAndWrite_converts_container_nodes_to_double_newline(string nodeName)
        {
            // Arrange
            HtmlNodeCollection html =
                GetHtmlDocument(string.Format("<{0}>text</{0}><{0}>text</{0}>text", nodeName)).DocumentNode.ChildNodes;

            // Act
            _textExtractor.ExtractTextAndWrite(html, _stringWriter);

            // Assert
            Assert.AreEqual(
                "text" + Environment.NewLine + Environment.NewLine + "text" + Environment.NewLine + Environment.NewLine +
                "text", _textBuilder.ToString());
        }

        [Test]
        [TestCase("div")]
        [TestCase("li")]
        [TestCase("ul")]
        public void ExtractTextAndWrite_converts_container_nodes_to_single_newline(string nodeName)
        {
            // Arrange
            HtmlNodeCollection html =
                GetHtmlDocument(string.Format("<{0}>text</{0}><{0}>text</{0}>text", nodeName)).DocumentNode.ChildNodes;

            // Act
            _textExtractor.ExtractTextAndWrite(html, _stringWriter);

            // Assert
            Assert.AreEqual("text" + Environment.NewLine + "text" + Environment.NewLine + "text",
                _textBuilder.ToString());
        }

        [Test]
        public void ExtractTextAndWrite_keeps_anchor_text()
        {
            // Arrange
            HtmlNodeCollection html = GetHtmlDocument("<a href='test'>test text</a>").DocumentNode.ChildNodes;

            // Act
            _textExtractor.ExtractTextAndWrite(html, _stringWriter);

            // Assert
            Assert.AreEqual("test text", _textBuilder.ToString());
        }

        [Test]
        [TestCase("script")]
        [TestCase("style")]
        [TestCase("button")]
        [TestCase("textarea")]
        [TestCase("select")]
        public void ExtractTextAndWrite_removes_non_text_nodes(string nodeName)
        {
            // Arrange
            HtmlNodeCollection html =
                GetHtmlDocument(string.Format("<{0}>text();</{0}>", nodeName)).DocumentNode.ChildNodes;

            // Act
            _textExtractor.ExtractTextAndWrite(html, _stringWriter);

            // Assert
            Assert.AreEqual(0, _textBuilder.Length);
        }

        [Test]
        public void ExtractTextAndWrite_strips_comments()
        {
            // Arrange
            HtmlNodeCollection html =
                GetHtmlDocument(string.Format("blah<!-- comment -->blah")).DocumentNode.ChildNodes;

            // Act
            _textExtractor.ExtractTextAndWrite(html, _stringWriter);

            // Assert
            Assert.AreEqual("blahblah", _textBuilder.ToString());
        }
    }
}
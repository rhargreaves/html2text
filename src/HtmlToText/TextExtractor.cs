using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace HtmlToText
{
    public class TextExtractor
    {
        private readonly HashSet<string> _elementNamesThatImplyNewLine = new HashSet<string>
        {
            "li",
            "div",
            "ul",
            "p",
            "h1",
            "h2",
            "h3",
            "h4"
        };

        private readonly HashSet<string> _formElementNames = new HashSet<string>
        {
            "label",
            "textarea",
            "button",
            "option",
            "select"
        };

        public void ExtractTextAndWrite(IEnumerable<HtmlNode> nodes, TextWriter writer)
        {
            foreach (HtmlNode node in nodes)
            {
                ExtractTextAndWrite(node, writer);
            }
        }

        private bool ExtractTextAndWrite(HtmlNode node, TextWriter writer)
        {
            if (node.NodeType == HtmlNodeType.Comment)
                return false;

            if (node.NodeType == HtmlNodeType.Text)
            {
                string text = node.InnerText;
                text = HtmlEntity.DeEntitize(text);
                text = Regex.Replace(text.Trim(), @"\s+", " ");

                writer.Write(text);
                return true;
            }

            if (node.Name == "script" || node.Name == "style" || _formElementNames.Contains(node.Name))
                return false;

            if (node.Name == "a")
            {
                HtmlNodeCollection siblings = node.ParentNode.ChildNodes;
                if (!siblings.Any(n => n.Name != "a" && !string.IsNullOrEmpty(node.InnerText)))
                    return false;
            }

            bool written = false;
            foreach (HtmlNode child in node.ChildNodes)
            {
                written = ExtractTextAndWrite(child, writer);
            }

            if (written && _elementNamesThatImplyNewLine.Contains(node.Name))
                writer.WriteLine();

            return written;
        }
    }
}
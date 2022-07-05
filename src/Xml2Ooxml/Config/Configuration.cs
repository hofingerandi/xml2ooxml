using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Xml2Ooxml.Config
{
    [XmlRoot]
    public class Configuration
    {
        public Configuration()
        {
            NamespaceEntries = new();
            NameReplacements = new();
            XPathsToExternalize = new();
            DocumentPath = "";
            MaxDepth = 10;
        }

        public int MaxDepth { get; set; }
        public string DocumentPath { get; set; }
        public string TargetFolder { get; set; }
        public List<XmlNamespaceEntry> NamespaceEntries { get; set; }
        public List<NameReplacement> NameReplacements { get; set; }
        [XmlArrayItem("XPath")]
        public List<XPathEntry> XPathsToExternalize { get; set; }

        internal static Configuration CreateDefault()
        {
            return new Configuration()
            {
                DocumentPath = @"C:\development\github\xml2ooxml\simple.xml",
                TargetFolder = @"C:\development\github\xml2ooxml\.out",
                NamespaceEntries = new()
                {
                    new() { Abbreviation = "plc", Namespace = "http://www.plcopen.org/xml/tc6_0200" },
                    new() { Abbreviation = "xhtml", Namespace = "http://www.w3.org/1999/xhtml" }
                },
                NameReplacements = new()
                {
                    new() { FullName = "http://www.3s-software.com/plcopenxml/", Abbreviation = "plc_" }
                },
                XPathsToExternalize = new()
                {
                   new("//plc:pou" ),
                   new("//plc:configuration" ),
                   new("//plc:sourcecode" ),
                   new("//plc:include" , "../../@name"),
                }
            };
        }

        //converter.RegisterTypeForExternalization("/plc:project/plc:contentHeader");
        //converter.RegisterTypeForExternalization("/plc:project/plc:contentHeader/plc:coordinateInfo");
        //converter.RegisterTypeForExternalization("/externalized_contentHeader/plc:coordinateInfo");
        //converter.RegisterTypeForExternalization("//plc:coordinateInfo//plc:scaling");
        /*
var els1 = doc.XPathSelectElements("/*[name()='project']", nsmgr);
var els2 = doc.XPathSelectElements("/plc:project", nsmgr);
var els3 = doc.XPathSelectElements("/project", nsmgr);,
//     "//plc:coordinateInfo//plc:scaling"
*/

        public class XmlNamespaceEntry
        {
            [XmlAttribute]
            public string Abbreviation { get; set; }

            [XmlAttribute]
            public string Namespace { get; set; }
        }

        public class NameReplacement
        {

            [XmlAttribute]
            public string FullName { get; set; }
            [XmlAttribute]
            public string Abbreviation { get; set; }
        }


    }

    [XmlRoot]
    public class XPathEntry
    {
        [Obsolete("For Xml-Serialization only", true)]
        public XPathEntry()
        {
        }

        public XPathEntry(string value, string selector = null)
        {
            Value = value;
            Selector = selector;
        }

        [XmlAttribute("selector")]
        public string Selector { get; set; }
        [XmlText]
        public string Value { get; set; }
    }
}

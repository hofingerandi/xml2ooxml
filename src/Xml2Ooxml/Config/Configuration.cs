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
        public List<string> XPathsToExternalize { get; set; }

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
                    "//plc:pou",
                    "//plc:configuration",
                    "//plc:sourcecode",
                    "//plc:include",
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
}

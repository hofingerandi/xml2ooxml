using System.Diagnostics;
using System.Xml.Linq;

namespace Xml2OoXml
{
    [DebuggerDisplay("{Document.Root.Name.LocalName}")]
    class DocToParse
    {
        public XDocument Document { get; set; }
        public bool Parsed { get; set; }
        public DocToParse ParentDoc { get; set; }
        public XElement ParentElement { get; set; }
        public string LocalFolder { get; set; }
        public string FullFolder { get; set; }
        public string FileName { get; internal set; }
        public string FullFilename { get; internal set; }
        public XElement OrigElement { get; internal set; }
        public string Content { get; internal set; }
        public string Extension => Document == null ? ".txt" : ".xml";
    }
}


using System.Xml.Linq;

namespace Xml2OoXml
{
    class DocToParse
    {
        public XDocument Document { get; set; }
        public bool Parsed { get; set; }
        public DocToParse ParentDoc { get; set; }
        public XElement ParentElement { get; set; }
    }
}


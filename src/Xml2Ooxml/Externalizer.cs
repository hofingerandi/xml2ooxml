using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Xml2Ooxml
{
    /// <summary>
    /// Find and remember, which elements should be externalized
    /// </summary>
    public class Externalizer
    {
        HashSet<string> _queriedXpaths = new();
        HashSet<string> _usedXPaths = new();
        List<XElement> _xpathElements = new();

        XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());

        public void RegisterNamespace(string prefix, string xmlNamespace)
        {
            _namespaceManager.AddNamespace(prefix, xmlNamespace);
        }

        public IEnumerable<XElement> RegisterElementsFromXPath(XDocument doc, string xPath)
        {
            _queriedXpaths.Add(xPath);
            var elements = doc.XPathSelectElements(xPath, _namespaceManager).Where(el => el != doc.Root);
            _xpathElements.AddRange(elements);
            if (elements.Any())
                _usedXPaths.Add(xPath);
            return elements;
        }

        /// <summary>
        /// XPaths that have been queried, but never yielded a match
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetUnusedXPaths()
        {
            foreach (var xpath in _queriedXpaths)
            {
                if (!_usedXPaths.Contains(xpath))
                    yield return xpath;
            }
        }

        public bool ShouldExternalize(XElement element)
        {
            if (element == null)
                return false;

            if (_xpathElements.Contains(element))
                return true;

            return false;
        }
    }
}

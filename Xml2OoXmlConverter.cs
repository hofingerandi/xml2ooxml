using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Xml.XPath;
using System.Xml;

namespace Xml2OoXml
{
    class Xml2OoXmlConverter
    {
        int MaxDepth = 8;
        List<string> _xpaths = new();
        List<XElement> _xpathElements = new();
        List<DocToParse> _docsToParse = new();
        List<DocToParse> _docsToStore = new();

        public void RegisterNamespace(string prefix, string xmlNamespace)
        {
            _namespaceManager.AddNamespace(prefix, xmlNamespace);
        }

        XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());

        public void RegisterTypeForExternalization(string xpath)
        {
            _xpaths.Add(xpath);
        }

        public void ConvertDocument(XDocument doc, DirectoryInfo targetFolder)
        {
            FindElementsFromXPath(doc);

            // During parsing, we create more and more documents that should be recursively split up
            _docsToParse.Add(new DocToParse() { Document = doc, Parsed = false });
            do
            {
                // Continue, until we parse all sub-documents
                var docToParse = _docsToParse.FirstOrDefault(d => !d.Parsed);
                if (docToParse == null)
                    break;

                ParseRecursively(docToParse, docToParse.Document.Root, 0);
                _docsToStore.Add(docToParse);
                docToParse.Parsed = true;
            }
            while (true);

            _docsToStore.Reverse();
            int i = 10;
            if (!targetFolder.Exists)
                targetFolder.Create();

            foreach (var docToStore in _docsToStore)
            {
                i++;
                string targetPath;
                if (docToStore.ParentElement != null)
                {
                    targetPath = Path.Combine(targetFolder.FullName, $"ex_{docToStore.ParentElement.Name.LocalName}_{i}.xml");
                    docToStore.ParentElement.SetAttributeValue("stored", targetPath);
                }
                else
                {
                    targetPath = Path.Combine(targetFolder.FullName, $"main_{i}.xml");
                }
                docToStore.Document.Save(targetPath);
            }
        }

        private void FindElementsFromXPath(XDocument doc)
        {
            foreach (var xpath in _xpaths)
            {
                var elements = doc.XPathSelectElements(xpath, _namespaceManager);
                _xpathElements.AddRange(elements);
                if (elements.Count() == 0)
                    Console.WriteLine($"No elements found that match '{xpath}'");
            }
        }


        public void ParseRecursively(DocToParse docToParse, XElement element, int depth)
        {
            if (element == null)
                return;

            if (depth > MaxDepth)
                return;

            if (ShouldExternalize(element, depth))
            {
                Externalize(docToParse, element);
                return;
            }

            Debug.WriteLine($"{new String('-', depth)}{element.Name.LocalName}");
            foreach (var node in element.Elements())
            {
                ParseRecursively(docToParse, node, depth + 1);
            }
        }

        private void Externalize(DocToParse docToParse, XElement element)
        {
            Debug.WriteLine($"Externalizing {element.Name.LocalName}");
            // TODO
            // * remember the folder, where the current doc is stored
            // * start creating new documents in its subfolder
            // * for testing, store the files directly
            // * probably, no files must be store in between
            var newDoc = new XDocument(new XElement($"externalized_{element.Name.LocalName}", element.Elements()));
            //newDoc.Save(@"C:\development\github\xml2ooxml\out_sub.xml");
            _docsToParse.Add(new DocToParse() { Document = newDoc, ParentDoc = docToParse, ParentElement = element });
            element.RemoveNodes();
            element.SetAttributeValue("externalized", true);
        }

        public bool ShouldExternalize(XElement element, int depth)
        {
            if (element == null)
                return false;

            if (_xpathElements.Contains(element))
                return true;

            return false;
        }
    }
}


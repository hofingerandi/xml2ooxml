using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace ConsoleApp1
{
    class DocToParse
    {
        public XDocument Document { get; set; }
        public bool Parsed { get; set; }
        public DocToParse ParentDoc { get; set; }
        public XElement ParentElement { get; set; }
    }

    class Xml2OoXmlConverter
    {
        List<Tuple<string, int>> _registry = new();
        List<DocToParse> _docsToParse = new();
        List<DocToParse> _docsToStore = new();

        public void ConvertDocument(XDocument doc, DirectoryInfo targetFolder)
        {
            _docsToParse.Add(new DocToParse() { Document = doc, Parsed = false });
            do
            {
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

        public void RegisterType(string localname, int depth)
        {
            _registry.Add(new Tuple<string, int>(localname, depth));
        }

        public void ParseRecursively(DocToParse docToParse, XElement element, int depth)
        {
            if (depth > 8)
                return;

            if (element == null)
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

            return _registry.Find(t => t.Item1 == element?.Name?.LocalName && t.Item2 == depth) != null;
        }
    }
}


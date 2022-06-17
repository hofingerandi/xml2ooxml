using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace ConsoleApp1
{
    class Xml2OoXmlConverter
    {
        List<Tuple<string, int>> _registry = new List<Tuple<string, int>>();

        public void RegisterType(string localname, int depth)
        {
            _registry.Add(new Tuple<string, int>(localname, depth));
        }

        public void ParseRecursively(XElement element, int depth)
        {
            if (depth > 3)
                return;

            if (element == null)
                return;

            if (ShouldExternalize(element, depth))
            {
                Externalize(element);
                return;
            }

            Debug.WriteLine($"{new String('-', depth)}{element.Name.LocalName}");
            foreach (var node in element.Elements())
            {
                ParseRecursively(node, depth + 1);
            }
        }

        private static void Externalize(XElement element)
        {
            Debug.WriteLine($"Externalizing {element.Name.LocalName}");
            // TODO
            // * store the newDoc in some container, remember its parent doc
            // * remember the folder, where the current doc is stored
            // * when parsing of the main document is finished, parse all newly created documents in the container
            // * start creating new documents in its subfolder
            // * for testing, store the files directly
            // * probably, no files must be store in between
            var newDoc = new XDocument(new XElement("externalized", element.Elements()));
            newDoc.Save(@"C:\development\github\xml2ooxml\out_sub.xml");
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


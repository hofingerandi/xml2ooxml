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

    // TODO: always create 2 subfolders from local name + parent name
    // datatypes/pou_dupli.xml
    // datatypes/pou_dupli/implementation/sourcecode.xml
    //
    // TODO: store content of elements without attributes directly, not as xml?



    class Xml2OoXmlConverter
    {
        int MaxDepth = 8;
        List<string> _xpaths = new();
        HashSet<string> _usedXPaths = new();
        List<XElement> _xpathElements = new();
        List<DocToParse> _docsToParse = new();
        List<DocToParse> _docsToStore = new();
        List<Tuple<string, string>> _nameReplacements = new();

        public void RegisterNamespace(string prefix, string xmlNamespace)
        {
            _namespaceManager.AddNamespace(prefix, xmlNamespace);
        }

        XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());

        internal void RegisterNameReplacement(string replace, string with)
        {
            _nameReplacements.Add(Tuple.Create(replace, with));
        }

        public void RegisterTypeForExternalization(string xpath)
        {
            _xpaths.Add(xpath);
        }

        public void ConvertDocument(XDocument doc, DirectoryInfo targetFolder)
        {
            // During parsing, we create more and more documents that should be recursively split up
            _docsToParse.Add(new DocToParse() { Document = doc, Parsed = false });
            do
            {
                // Continue, until we parse all sub-documents
                var docToParse = _docsToParse.FirstOrDefault(d => !d.Parsed);
                if (docToParse == null)
                    break;

                FindElementsFromXPath(docToParse.Document);
                ParseRecursively(docToParse, docToParse.Document.Root, 0);
                _docsToStore.Add(docToParse);
                docToParse.Parsed = true;
            }
            while (true);

            LogUnusedXPaths();

            CreateFileAndFoldernames();

            StoreLinksInParents();

            CreateFolderStructure(targetFolder);

            StoreFiles(targetFolder);
        }

        private void StoreLinksInParents()
        {
            foreach (var docToStore in _docsToStore)
            {
                docToStore.OrigElement?.SetAttributeValue("ext", docToStore.FullFilename);
            }
        }

        private void CreateFileAndFoldernames()
        {
            CreateLocalNames();
            ConvertLocalToGlobalNames();
            MakeFilenamesUnique();
        }

        private void CreateLocalNames()
        {
            // which filename should this doc have
            foreach (var docToStore in _docsToStore)
            {
                if (docToStore.ParentElement != null)
                {
                    Debug.Assert(docToStore.ParentDoc != null);
                    docToStore.FileName = GetValidFilename(docToStore.Document.Root);
                }
                else
                {
                    docToStore.FileName = GetValidFilename(docToStore.Document.Root);
                    Console.WriteLine($"Filename: {docToStore.FileName}");

                }
            }

            // in which folder should this doc be stored?
            // may depend on filename of parent, so two separate loops
            foreach (var docToStore in _docsToStore.Where(d => d.ParentElement != null))
            {
                docToStore.LocalFolder = docToStore.ParentDoc.FileName + "\\" + GetValidFilename(docToStore.ParentElement);
                Console.WriteLine($"Storage: {docToStore.LocalFolder}/{docToStore.FileName}");
            }
        }

        // Check document hierarchy, and find the full folder structure
        private void ConvertLocalToGlobalNames()
        {
            foreach (var docToStore in _docsToStore)
            {
                string folder = docToStore.LocalFolder;
                var parent = docToStore;
                while (parent.ParentDoc?.LocalFolder != null)
                {
                    folder = parent.ParentDoc.LocalFolder + "/" + folder;
                    parent = parent.ParentDoc;
                }
                docToStore.FullFolder = folder;
                Console.WriteLine($"{docToStore.FullFolder}/{docToStore.FileName}.xml");
            }
        }

        private void MakeFilenamesUnique()
        {
            HashSet<string> knownPaths = new();
            foreach (var docToStore in _docsToStore)
            {
                if (docToStore.ParentDoc == null)
                {
                    // main document is always unique
                    docToStore.FullFilename = docToStore.FileName;
                    continue;
                }

                var currentPath = Path.Combine(docToStore.FullFolder, docToStore.FileName);
                var validPath = currentPath;
                int i = 0;
                while (knownPaths.Contains(validPath))
                {
                    validPath = currentPath + $"_{++i:D2}";
                }
                docToStore.FullFilename = validPath;
                knownPaths.Add(validPath);
                Console.WriteLine(validPath);
            }
        }

        private void CreateFolderStructure(DirectoryInfo targetFolder)
        {
            if (!targetFolder.Exists)
                targetFolder.Create();

            foreach (var doc in _docsToStore)
            {
                if (doc.FullFolder == null)
                    continue;

                var folder = Path.Combine(targetFolder.FullName, doc.FullFolder);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
        }

        private void StoreFiles(DirectoryInfo targetFolder)
        {
            foreach (var doc in _docsToStore)
            {
                var fullFilename = Path.Combine(targetFolder.FullName, doc.FullFilename + ".xml");
                Console.WriteLine(fullFilename);
                doc.Document.Save(fullFilename);
            }
        }

        private string GetValidFilename(XElement xElement)
        {
            var nameAttr = xElement.Attribute("name");
            string result;
            if (nameAttr != null)
            {
                var nameValue = nameAttr.Value;
                foreach (var item in _nameReplacements)
                {
                    nameValue = nameValue.Replace(item.Item1, item.Item2);
                }
                result = xElement.Name.LocalName + "_" + nameValue;
            }
            else
            {
                result = xElement.Name.LocalName;
            }
            return MakeValidFileName(result);
        }

        // https://stackoverflow.com/a/847251/821134
        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "");
        }

        private void LogUnusedXPaths()
        {
            foreach (var xpath in _xpaths)
            {
                if (!_usedXPaths.Contains(xpath))
                    Console.WriteLine($"No elements found that match '{xpath}'");
            }
        }

        private void FindElementsFromXPath(XDocument doc)
        {
            foreach (var xpath in _xpaths)
            {
                var elements = doc.XPathSelectElements(xpath, _namespaceManager).Where(el => el != doc.Root);
                _xpathElements.AddRange(elements);
                if (elements.Count() != 0)
                    _usedXPaths.Add(xpath);
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

            //Debug.WriteLine($"{new String('-', depth)}{element.Name.LocalName}");
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
            var newDoc = new XDocument(new XElement(element)); //)$"{element.Name}", element.Elements()));
            //newDoc.Save(@"C:\development\github\xml2ooxml\out_sub.xml");
            _docsToParse.Add(new DocToParse() { Document = newDoc, OrigElement = element, ParentDoc = docToParse, ParentElement = element.Parent });
            element.RemoveNodes();
            element.RemoveAttributes();
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


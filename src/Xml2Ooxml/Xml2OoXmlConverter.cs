using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Xml2Ooxml;
using Xml2Ooxml.Config;

namespace Xml2OoXml
{
    class Xml2OoXmlConverter
    {
        List<XPathEntry> _xpaths = new();
        HashSet<string> _usedXPaths = new();
        List<XElement> _xpathElements = new();
        List<DocToParse> _docsToParse = new();
        List<DocToParse> _docsToStore = new();
        NameHandling _nameHandling = new ();

        public int MaxDepth { get; set; }

        public void RegisterNamespace(string prefix, string xmlNamespace)
        {
            _namespaceManager.AddNamespace(prefix, xmlNamespace);
        }

        XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());

        internal void RegisterNameReplacement(string replace, string with)
        {
            _nameHandling.RegisterNameReplacement(replace, with);
        }

        public void RegisterTypeForExternalization(XPathEntry xpath)
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

            Console.WriteLine($"{_docsToStore.Count} documents stored");
        }

        private void StoreLinksInParents()
        {
            foreach (var docToStore in _docsToStore)
            {
                docToStore.OrigElement?.SetAttributeValue("ext", docToStore.FullFilename + docToStore.Extension);
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
                    if (docToStore.Document != null)
                    {
                        docToStore.FileName = GetValidFilename(docToStore.Document.Root);
                    }
                    else
                    {
                        docToStore.FileName = GetValidFilename(docToStore.OrigElement);
                    }
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
                if (docToStore.ParentDoc?.Document?.Root == docToStore.ParentElement)
                {
                    docToStore.LocalFolder = docToStore.ParentDoc.FileName;
                }
                else
                {
                    docToStore.LocalFolder = Path.Combine(docToStore.ParentDoc.FileName, GetValidFilename(docToStore.ParentElement));
                }
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
                    folder = Path.Combine(parent.ParentDoc.LocalFolder, folder);
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
                var fullFilename = Path.Combine(targetFolder.FullName, doc.FullFilename + doc.Extension);
                Console.WriteLine(fullFilename);
                if (doc.Document != null)
                {
                    doc.Document.Save(fullFilename);
                }
                else
                {
                    Debug.Assert(doc.Content != null);
                    File.WriteAllText(fullFilename, doc.Content);
                }
            }
        }

        private string GetValidFilename(XElement xElement)
        {
            return _nameHandling.GetValidFileName(xElement);
        }


        private void LogUnusedXPaths()
        {
            foreach (var xpath in _xpaths)
            {
                if (!_usedXPaths.Contains(xpath.Value))
                    Console.WriteLine($"No elements found that match '{xpath}'");
            }
        }

        private void FindElementsFromXPath(XDocument doc)
        {
            foreach (var xpath in _xpaths)
            {
                var elements = doc.XPathSelectElements(xpath.Value, _namespaceManager).Where(el => el != doc.Root);
                _xpathElements.AddRange(elements);
                if (!String.IsNullOrEmpty(xpath.Selector))
                {
                    foreach (var element in elements)
                    {
                        _nameHandling.IdentifySpecialName(element, xpath.Selector);
                    }
                }
                if (elements.Any())
                    _usedXPaths.Add(xpath.Value);
            }
        }

        public void ParseRecursively(DocToParse docToParse, XElement element, int depth)
        {
            if (element == null)
                return;

            if (depth > MaxDepth)
                return;

            if (ShouldExternalize(element))
            {
                Externalize(docToParse, element);
                return;
            }

            foreach (var node in element.Elements())
            {
                ParseRecursively(docToParse, node, depth + 1);
            }
        }

        private void Externalize(DocToParse docToParse, XElement element)
        {
            Debug.WriteLine($"Externalizing {element.Name.LocalName}");
            if (IsContentOnlyElement(element))
            {
                // content is stored directly
                _docsToStore.Add(new DocToParse() { Document = null, OrigElement = element, ParentDoc = docToParse, ParentElement = element.Parent, Content = element.Value });
                element.SetValue(String.Empty);
            }
            else
            {
                // element is externalized; new document parsed recursively
                var newElement = new XElement(element);
                var newDoc = new XDocument(newElement);
                _nameHandling.RegisterRelatedElement(element, newElement);
                _docsToParse.Add(new DocToParse() { Document = newDoc, OrigElement = element, ParentDoc = docToParse, ParentElement = element.Parent });
                element.RemoveNodes();
                element.RemoveAttributes();
            }
        }

        // An element that only consists of content
        private static bool IsContentOnlyElement(XElement element)
        {
            if (element.HasElements)
                return false;

            if (!element.HasAttributes)
                return true;

            if (element.Attributes().Count() == 1 && element.FirstAttribute.IsNamespaceDeclaration)
            {
                return true;
            }
            return false;
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


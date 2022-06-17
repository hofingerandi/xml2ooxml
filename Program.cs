using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace ConsoleApp1
{
    internal class Program
    {

        static List<Tuple<string, int>> _registry = new List<Tuple<string, int>>();
        static void Main(string[] args)
        {
            var doc = XDocument.Load(@"C:\development\github\xml2ooxml\input.xml");

            var converter = new Xml2OoXmlConverter();
            converter.RegisterType("coordinateInfo", 2);
            converter.RegisterType("pou", 7);
            DirectoryInfo targetFolder = new DirectoryInfo(@"C:\development\github\xml2ooxml\out");
            converter.ConvertDocument(doc, targetFolder);
        }
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace ConsoleApp1
{
    internal class Program
    {

        static List<Tuple<string, int>> _registry = new List<Tuple<string, int>>();
        static int Main(string[] args)
        {
            var doc = XDocument.Load(@"C:\development\github\xml2ooxml\input.xml");
            try
            {
                var converter = new Xml2OoXmlConverter();
                converter.RegisterNamespace("plc", "http://www.plcopen.org/xml/tc6_0200");
                converter.RegisterTypeForExternalization("/plc:project/plc:contentHeader/plc:coordinateInfo");
                converter.RegisterTypeForExternalization("//plc:data/plc:pou");
                DirectoryInfo targetFolder = new DirectoryInfo(@"C:\development\github\xml2ooxml\out");
                converter.ConvertDocument(doc, targetFolder);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}

/*
var els1 = doc.XPathSelectElements("/*[name()='project']", nsmgr);
var els2 = doc.XPathSelectElements("/plc:project", nsmgr);
var els3 = doc.XPathSelectElements("/project", nsmgr);
*/


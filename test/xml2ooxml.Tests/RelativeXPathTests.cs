using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using xml2ooxml.Tests;

namespace Xml2Ooxml.Tests
{
    [TestClass()]
    public class RelativeXPathTests
    {
        /// <summary>
        /// Combinations of xpath for selecting an element, and xpath for determining its name
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="selector"></param>
        /// <param name="expectedFileName"></param>
        [DataTestMethod()]
        // name attribute of parent
        [DataRow("//plc:pou/plc:body/plc:ST/xhtml:xhtml", @"../../../@name", "xhtml_PLC_PRG")]
        // name of element
        [DataRow("//plc:pou/plc:body/plc:ST/xhtml:xhtml", @"name(../../..)", "xhtml_pou")]
        // various attributes
        [DataRow("//plc:pou/plc:body/plc:ST/xhtml:xhtml", @"concat(../../@type,'_',../@lang,'_',name())", "xhtml_methodBody_structText_xhtml")]
        // iterating through the tree
        [DataRow("//plc:pou/plc:body/plc:ST/xhtml:xhtml", @"../../..//plc:interface/@iType", "xhtml_explicit")]
        public void CheckFileName(string xpath, string selector, string expectedFileName)
        {
            var doc = XDocument.Parse(SampleData.VisuTest);
            var ex = new Externalizer();
            var nh = new NameHandling();

            ex.RegisterNamespace("plc", "http://www.plcopen.org/xml/tc6_0200");
            ex.RegisterNamespace("xhtml", "http://www.w3.org/1999/xhtml");
            nh.RegisterNamespace("plc", "http://www.plcopen.org/xml/tc6_0200");
            nh.RegisterNamespace("xhtml", "http://www.w3.org/1999/xhtml");

            var els = ex.RegisterElementsFromXPath(doc, xpath);
            Assert.IsNotNull(els);
            var el = els.First();
            nh.FindSpecialName(el, selector);
            var fn = nh.GetValidFileName(el);

            Assert.AreEqual(expectedFileName, fn);
        }
    }
}
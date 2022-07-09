using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xml2Ooxml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xml2Ooxml.Tests
{
    [TestClass()]
    public class NameHandlingTests
    {
        [TestMethod()]
        public void SimpleElement()
        {
            var element = new XElement(XName.Get("MyElement", "MyNamespace"));
            var target = new NameHandling();
            var fn = target.GetValidFileName(element);
            Assert.AreEqual("MyElement", fn);
        }

        [TestMethod()]
        public void Element_WithNameAttr()
        {
            var element = new XElement(XName.Get("MyElement", "MyNamespace"));
            element.SetAttributeValue("name", "MyAttribute");
            var target = new NameHandling();
            var fn = target.GetValidFileName(element);
            Assert.AreEqual("MyElement_MyAttribute", fn);
        }

        [TestMethod()]
        public void Element_WithNonFilenameAttr()
        {
            var element = new XElement(XName.Get("MyElement", "MyNamespace"));
            element.SetAttributeValue("name", "http://some.url/MyAttribute");
            var target = new NameHandling();
            var fn = target.GetValidFileName(element);
            Assert.AreEqual("MyElement_httpsome.urlMyAttribute", fn);
        }

        [TestMethod()]
        public void Element_WithNonFilenameAttr_WithReplacements()
        {
            var element = new XElement(XName.Get("MyElement", "MyNamespace"));
            element.SetAttributeValue("name", "http://some.url/MyAttribute");
            var target = new NameHandling();
            target.RegisterNameReplacement("http://some.url/", "UrlNs_");
            var fn = target.GetValidFileName(element);
            Assert.AreEqual("MyElement_UrlNs_MyAttribute", fn);
        }

        [TestMethod()]
        public void Element_WithSpecialAttr()
        {
            var element = new XElement(XName.Get("MyElement", "MyNamespace"));
            element.SetAttributeValue("kind", "BeSoKind");
            var target = new NameHandling();
            target.IdentifySpecialName(element, "@kind");
            var fn = target.GetValidFileName(element);
            Assert.AreEqual("MyElement_BeSoKind", fn);
        }

        [TestMethod()]
        public void Element_ComplexXpath()
        {
            var element = new XElement(XName.Get("MyElement", "MyNamespace"));
            element.SetAttributeValue("kind", "Be");
            element.SetAttributeValue("name", "SoKind");
            var target = new NameHandling();
            target.IdentifySpecialName(element, "concat(@kind,@name)");
            var fn = target.GetValidFileName(element);
            Assert.AreEqual("MyElement_BeSoKind", fn);
        }

        [TestMethod()]
        public void Element_ComplexXpath2()
        {
            var element = new XElement(XName.Get("MyElement", "MyNamespace"));
            element.SetAttributeValue("kind", "Be");
            element.SetAttributeValue("name", "SoKind");
            var target = new NameHandling();
            target.IdentifySpecialName(element, @"concat('-',name(),'-',@kind,'-',@name)");
            var fn = target.GetValidFileName(element);
            Assert.AreEqual("MyElement_-MyElement-Be-SoKind", fn);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Xml2Ooxml
{
    /// <summary>
    /// Find good names for XElements
    /// </summary>
    public class NameHandling
    {
        Dictionary<XElement, string> _specialNames = new();
        List<Tuple<string, string>> _nameReplacements = new();
        XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());

        public bool UseLocalNameAsPrefix { get; set; } = true;

        /// <summary>
        /// Enable namespaces in xpath-selectors
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="xmlNamespace"></param>
        public void RegisterNamespace(string prefix, string xmlNamespace)
        {
            _namespaceManager.AddNamespace(prefix, xmlNamespace);
        }


        /// <summary>
        /// Some auto-generated names might contain clumsy substrings, e.g. urls.
        /// </summary>
        /// <param name="replace"></param>
        /// <param name="with"></param>
        public void RegisterNameReplacement(string replace, string with)
        {
            _nameReplacements.Add(Tuple.Create(replace, with));
        }

        public void FindSpecialNames(IEnumerable<XElement> elements, string selector)
        {
            if (String.IsNullOrEmpty(selector))
                return;

            foreach (var element in elements)
            {
                FindSpecialName(element, selector);
            }
        }

        /// <summary>
        /// Use the selector to determine a meaningful special name
        /// </summary>
        /// <param name="element"></param>
        /// <param name="selector"></param>
        public void FindSpecialName(XElement element, string selector)
        {
            if (String.IsNullOrEmpty(selector))
                return;

            var xpe = element.XPathEvaluate(selector, _namespaceManager);
            if (xpe is IEnumerable<object> ieo)
            {
                if (ieo?.FirstOrDefault() is XAttribute attribute)
                {
                    _specialNames[element] = attribute.Value;
                }
            }
            else if (xpe is string s)
            {
                _specialNames[element] = s;
            }
        }

        /// <summary>
        /// Use the same special name for <see cref="newElement"/> as for <see cref="element"/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="newElement"></param>
        public void RegisterRelatedElement(XElement element, XElement newElement)
        {
            var specialName = GetSpecialNameOrNull(element);
            if (specialName != null)
            {
                _specialNames[newElement] = specialName;
            }
        }

        /// <summary>
        /// Find a meaningful name for xElement
        /// </summary>
        /// <param name="xElement"></param>
        /// <returns></returns>
        public string GetValidFileName(XElement xElement)
        {
            string result;
            string specialName = GetSpecialNameOrNull(xElement);

            if (!String.IsNullOrEmpty(specialName))
            {
                if (UseLocalNameAsPrefix)
                {
                    result = xElement.Name.LocalName + "_" + specialName;
                }
                else
                {
                    result = specialName;
                }
            }
            else
            {
                result = xElement.Name.LocalName;
            }
            foreach (var item in _nameReplacements)
            {
                result = result.Replace(item.Item1, item.Item2);
            }
            return MakeValidFileName(result);
        }

        private string GetSpecialNameOrNull(XElement xElement)
        {
            if (_specialNames.TryGetValue(xElement, out var specialName))
            {
                return specialName;
            }
            else
            {
                var nameAttr = xElement.Attribute("name");
                return nameAttr?.Value;
            }
        }

        /// <summary>
        /// https://stackoverflow.com/a/847251/821134
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "");
        }
    }
}

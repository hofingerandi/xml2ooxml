using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Xml2Ooxml
{
    public class NameHandling
    {
        Dictionary<XElement, string> _specialNames = new();
        List<Tuple<string, string>> _nameReplacements = new();

        /// <summary>
        /// Use the selector to determine a meaningful special name
        /// </summary>
        /// <param name="element"></param>
        /// <param name="selector"></param>
        public void IdentifySpecialName(XElement element, string selector)
        {
            var s = element.XPathEvaluate(selector) as IEnumerable<object>;
            if (s?.FirstOrDefault() is XAttribute attribute)
            {
                _specialNames[element] = attribute.Value;
            }
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
                result = xElement.Name.LocalName + "_" + specialName;
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

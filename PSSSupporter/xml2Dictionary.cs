using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace PSSSupporter {
    //TODO: crear clases específicas para hacer de alojamiento similar a un XML
    public static class xml2Dictionary {
        public static object Parse(XElement root) {
            var result = new Dictionary<string, object>();
            result.Add(root.Name.LocalName, ParseElement(root));
            return result;
        }
        public static object Parse(string toParse) {
            var doc = XDocument.Parse(toParse);
            return Parse(doc.Root);
        }
        private static object ParseElement(XElement element) {
            var result = new Dictionary<string, object>();
            List<string> names = new List<string>();
            Dictionary<string, string> name2Dictionary = new Dictionary<string, string>();
            foreach (var e in element.Elements()) {
                string name = e.Name.LocalName;
                if (names.Contains(name)) {
                    string dictName = "l_" + name;
                    if (!result.Keys.Contains(dictName)) {
                        result.Add(dictName, new List<object>());
                        object o = result[name];
                        ((List<object>)result[dictName]).Add(o);
                        result.Remove(name);
                    }
                    ((List<object>)result[dictName]).Add(ParseElement(e));
                } else {
                    names.Add(name);
                    result.Add(e.Name.LocalName, ParseElement(e));
                }
                //dict.Add(subelement.Value, ParseValue(subelement.ElementsAfterSelf().First()));
            }
            foreach (var a in element.Attributes()) {
                string aName = a.Name.LocalName;
                aName = "a_" + aName;
                while (result.Keys.Contains(aName)) {
                    aName = "a_" + aName;
                }
                result.Add(aName, a.Value);
            }
            if (!string.IsNullOrEmpty(element.Value)) { result.Add("v_", element.Value); }
            //switch (result.Count) {
            //case 1:
            //return dict.Values.First();
            //case 0:
            //return null;
            //}
            return result;
        }
    }
}

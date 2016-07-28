using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;


namespace LegendsOfDescent
{
    public static class XmlHelper
    {
        public static XDocument Load(string filePath)
        {
#if SILVERLIGHT
            return XDocument.Load(Application.GetResourceStream(new Uri("/LoDSilverlight;component/" + filePath, UriKind.Relative)).Stream);
#else
            return XDocument.Load(filePath);
#endif
        }

        public static int IntValue(this XAttribute attr)
        {
            int val;
            if (attr == null || attr.Value == null || !int.TryParse(attr.Value, out val))
            {
                val = 0;
            }
            return val;
        }
    }
}

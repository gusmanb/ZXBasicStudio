using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ZXBasicStudio.LanguageDefinitions
{
    public abstract class LanguageDefinitionBase
    {
        public abstract string XshdDefinition { get; }
        public Stream AsStream { get { return new MemoryStream(Encoding.UTF8.GetBytes(XshdDefinition)); } }
        public XmlReader AsReader
        {
            get
            {
                var stream = AsStream;
                var reader = new XmlTextReader(stream);
                return reader; 
            }
        }
    }
}

using System.Collections.Generic;
using System.Xml.Serialization;

namespace BigDLL4221.Extensions
{
    public class EtcRoot
    {
        [XmlElement("Text")] public List<EtcText> Text;
    }

    public class EtcText
    {
        [XmlText] public string Desc;
        [XmlAttribute("ID")] public string ID;
    }
}
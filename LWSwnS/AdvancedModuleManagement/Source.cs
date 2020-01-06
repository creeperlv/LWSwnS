using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace AdvancedModuleManagement
{
    [Serializable,XmlRoot("Source")]
    public class Source
    {
        [XmlElement("SourceName")]
        public string Name;
        [XmlElement("PackageNames")]
        public List<string> PackageName=new List<string>();
        [XmlElement("PackageFiles")]
        public List<string> PackageFile=new List<string>();
    }
}

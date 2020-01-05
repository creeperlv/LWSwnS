using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace AdvancedModuleManagement
{
    [Serializable,XmlRoot("Package")]
    public class Package
    {
        public string Name;
        public string ID;
        public string Version;
        public string MainDLL;
        public string OriginalSource;
    }
}

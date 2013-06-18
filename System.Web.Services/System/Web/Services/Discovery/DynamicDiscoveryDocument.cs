namespace System.Web.Services.Discovery
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Text;
    using System.Xml.Serialization;

    [XmlRoot("dynamicDiscovery", Namespace="urn:schemas-dynamicdiscovery:disco.2000-03-17")]
    public sealed class DynamicDiscoveryDocument
    {
        private ExcludePathInfo[] excludePaths = new ExcludePathInfo[0];
        public const string Namespace = "urn:schemas-dynamicdiscovery:disco.2000-03-17";

        public static DynamicDiscoveryDocument Load(Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DynamicDiscoveryDocument));
            return (DynamicDiscoveryDocument) serializer.Deserialize(stream);
        }

        public void Write(Stream stream)
        {
            new XmlSerializer(typeof(DynamicDiscoveryDocument)).Serialize((TextWriter) new StreamWriter(stream, new UTF8Encoding(false)), this);
        }

        [XmlElement("exclude", typeof(ExcludePathInfo))]
        public ExcludePathInfo[] ExcludePaths
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.excludePaths;
            }
            set
            {
                if (value == null)
                {
                    value = new ExcludePathInfo[0];
                }
                this.excludePaths = value;
            }
        }
    }
}


namespace System.Xml.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Xml;

    public class XStreamingElement
    {
        internal object content;
        internal XName name;

        public XStreamingElement(XName name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.name = name;
        }

        public XStreamingElement(XName name, object content) : this(name)
        {
            this.content = (content is List<object>) ? new object[] { content } : content;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XStreamingElement(XName name, params object[] content) : this(name)
        {
            this.content = content;
        }

        public void Add(object content)
        {
            if (content != null)
            {
                List<object> list = this.content as List<object>;
                if (list == null)
                {
                    list = new List<object>();
                    if (this.content != null)
                    {
                        list.Add(this.content);
                    }
                    this.content = list;
                }
                list.Add(content);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Add(params object[] content)
        {
            this.Add(content);
        }

        private string GetXmlString(SaveOptions o)
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                XmlWriterSettings settings = new XmlWriterSettings {
                    OmitXmlDeclaration = true
                };
                if ((o & SaveOptions.DisableFormatting) == SaveOptions.None)
                {
                    settings.Indent = true;
                }
                if ((o & SaveOptions.OmitDuplicateNamespaces) != SaveOptions.None)
                {
                    settings.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
                }
                using (XmlWriter writer2 = XmlWriter.Create(writer, settings))
                {
                    this.WriteTo(writer2);
                }
                return writer.ToString();
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Save(Stream stream)
        {
            this.Save(stream, SaveOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Save(TextWriter textWriter)
        {
            this.Save(textWriter, SaveOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Save(string fileName)
        {
            this.Save(fileName, SaveOptions.None);
        }

        public void Save(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteStartDocument();
            this.WriteTo(writer);
            writer.WriteEndDocument();
        }

        public void Save(Stream stream, SaveOptions options)
        {
            XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
            using (XmlWriter writer = XmlWriter.Create(stream, xmlWriterSettings))
            {
                this.Save(writer);
            }
        }

        public void Save(TextWriter textWriter, SaveOptions options)
        {
            XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
            using (XmlWriter writer = XmlWriter.Create(textWriter, xmlWriterSettings))
            {
                this.Save(writer);
            }
        }

        public void Save(string fileName, SaveOptions options)
        {
            XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
            using (XmlWriter writer = XmlWriter.Create(fileName, xmlWriterSettings))
            {
                this.Save(writer);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override string ToString()
        {
            return this.GetXmlString(SaveOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public string ToString(SaveOptions options)
        {
            return this.GetXmlString(options);
        }

        public void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            new StreamingElementWriter(writer).WriteStreamingElement(this);
        }

        public XName Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.name = value;
            }
        }
    }
}


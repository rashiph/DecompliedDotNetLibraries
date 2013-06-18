namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Xml.Serialization;

    internal class XmlSerializerContract : XmlSerializerImplementation
    {
        private Hashtable readMethods;
        private Hashtable typedSerializers;
        private Hashtable writeMethods;

        public override bool CanSerialize(Type type)
        {
            return (type == typeof(MetadataSet));
        }

        public override XmlSerializer GetSerializer(Type type)
        {
            if (type == typeof(MetadataSet))
            {
                return new MetadataSetSerializer();
            }
            return null;
        }

        public override XmlSerializationReader Reader
        {
            get
            {
                return new XmlSerializationReaderMetadataSet();
            }
        }

        public override Hashtable ReadMethods
        {
            get
            {
                if (this.readMethods == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable["System.ServiceModel.Description.MetadataSet:http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata:True:"] = "Read68_Metadata";
                    if (this.readMethods == null)
                    {
                        this.readMethods = hashtable;
                    }
                }
                return this.readMethods;
            }
        }

        public override Hashtable TypedSerializers
        {
            get
            {
                if (this.typedSerializers == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable.Add("System.ServiceModel.Description.MetadataSet:http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata:True:", new MetadataSetSerializer());
                    if (this.typedSerializers == null)
                    {
                        this.typedSerializers = hashtable;
                    }
                }
                return this.typedSerializers;
            }
        }

        public override Hashtable WriteMethods
        {
            get
            {
                if (this.writeMethods == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable["System.ServiceModel.Description.MetadataSet:http://schemas.xmlsoap.org/ws/2004/09/mex:Metadata:True:"] = "Write68_Metadata";
                    if (this.writeMethods == null)
                    {
                        this.writeMethods = hashtable;
                    }
                }
                return this.writeMethods;
            }
        }

        public override XmlSerializationWriter Writer
        {
            get
            {
                return new XmlSerializationWriterMetadataSet();
            }
        }
    }
}


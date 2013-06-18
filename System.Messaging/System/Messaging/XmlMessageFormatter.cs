namespace System.Messaging
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlMessageFormatter : IMessageFormatter, ICloneable
    {
        private Hashtable targetSerializerTable;
        private string[] targetTypeNames;
        private Type[] targetTypes;
        private bool typeNamesAdded;
        private bool typesAdded;

        public XmlMessageFormatter()
        {
            this.targetSerializerTable = new Hashtable();
            this.TargetTypes = new Type[0];
            this.TargetTypeNames = new string[0];
        }

        public XmlMessageFormatter(string[] targetTypeNames)
        {
            this.targetSerializerTable = new Hashtable();
            this.TargetTypeNames = targetTypeNames;
            this.TargetTypes = new Type[0];
        }

        public XmlMessageFormatter(Type[] targetTypes)
        {
            this.targetSerializerTable = new Hashtable();
            this.TargetTypes = targetTypes;
            this.TargetTypeNames = new string[0];
        }

        public bool CanRead(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            this.CreateTargetSerializerTable();
            XmlTextReader xmlReader = new XmlTextReader(message.BodyStream) {
                WhitespaceHandling = WhitespaceHandling.Significant,
                DtdProcessing = DtdProcessing.Prohibit
            };
            bool flag = false;
            foreach (XmlSerializer serializer in this.targetSerializerTable.Values)
            {
                if (serializer.CanDeserialize(xmlReader))
                {
                    flag = true;
                    break;
                }
            }
            message.BodyStream.Position = 0L;
            return flag;
        }

        public object Clone()
        {
            XmlMessageFormatter formatter = new XmlMessageFormatter {
                targetTypes = this.targetTypes,
                targetTypeNames = this.targetTypeNames,
                typesAdded = this.typesAdded,
                typeNamesAdded = this.typeNamesAdded
            };
            foreach (Type type in this.targetSerializerTable.Keys)
            {
                formatter.targetSerializerTable[type] = new XmlSerializer(type);
            }
            return formatter;
        }

        private void CreateTargetSerializerTable()
        {
            if (!this.typeNamesAdded)
            {
                for (int i = 0; i < this.targetTypeNames.Length; i++)
                {
                    Type type = Type.GetType(this.targetTypeNames[i], true);
                    if (type != null)
                    {
                        this.targetSerializerTable[type] = new XmlSerializer(type);
                    }
                }
                this.typeNamesAdded = true;
            }
            if (!this.typesAdded)
            {
                for (int j = 0; j < this.targetTypes.Length; j++)
                {
                    this.targetSerializerTable[this.targetTypes[j]] = new XmlSerializer(this.targetTypes[j]);
                }
                this.typesAdded = true;
            }
            if (this.targetSerializerTable.Count == 0)
            {
                throw new InvalidOperationException(System.Messaging.Res.GetString("TypeListMissing"));
            }
        }

        public object Read(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            this.CreateTargetSerializerTable();
            XmlTextReader xmlReader = new XmlTextReader(message.BodyStream) {
                WhitespaceHandling = WhitespaceHandling.Significant,
                DtdProcessing = DtdProcessing.Prohibit
            };
            foreach (XmlSerializer serializer in this.targetSerializerTable.Values)
            {
                if (serializer.CanDeserialize(xmlReader))
                {
                    return serializer.Deserialize(xmlReader);
                }
            }
            throw new InvalidOperationException(System.Messaging.Res.GetString("InvalidTypeDeserialization"));
        }

        public void Write(Message message, object obj)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Stream stream = new MemoryStream();
            Type key = obj.GetType();
            XmlSerializer serializer = null;
            if (this.targetSerializerTable.ContainsKey(key))
            {
                serializer = (XmlSerializer) this.targetSerializerTable[key];
            }
            else
            {
                serializer = new XmlSerializer(key);
                this.targetSerializerTable[key] = serializer;
            }
            serializer.Serialize(stream, obj);
            message.BodyStream = stream;
            message.BodyType = 0;
        }

        [MessagingDescription("XmlMsgTargetTypeNames")]
        public string[] TargetTypeNames
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetTypeNames;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.typeNamesAdded = false;
                this.targetTypeNames = value;
            }
        }

        [Browsable(false), MessagingDescription("XmlMsgTargetTypes"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Type[] TargetTypes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetTypes;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.typesAdded = false;
                this.targetTypes = value;
            }
        }
    }
}


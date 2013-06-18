namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;

    [Serializable]
    internal sealed class InternalSoapMessage : ISerializable, IFieldInfo
    {
        internal Hashtable keyToNamespaceTable;
        internal string methodName;
        internal string[] paramNames;
        internal Type[] paramTypes;
        internal object[] paramValues;
        internal string xmlNameSpace;

        internal InternalSoapMessage()
        {
        }

        internal InternalSoapMessage(SerializationInfo info, StreamingContext context)
        {
            this.SetObjectData(info, context);
        }

        internal InternalSoapMessage(string methodName, string xmlNameSpace, string[] paramNames, object[] paramValues, Type[] paramTypes)
        {
            this.methodName = methodName;
            this.xmlNameSpace = xmlNameSpace;
            this.paramNames = paramNames;
            this.paramValues = paramValues;
            this.paramTypes = paramTypes;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            object[] paramValues = this.paramValues;
            info.FullTypeName = this.methodName;
            if (this.xmlNameSpace != null)
            {
                info.AssemblyName = this.xmlNameSpace;
            }
            string name = null;
            if (this.paramValues != null)
            {
                for (int i = 0; i < this.paramValues.Length; i++)
                {
                    if ((this.paramNames != null) && (this.paramNames[i] == null))
                    {
                        name = "param" + i;
                    }
                    else
                    {
                        name = this.paramNames[i];
                    }
                    info.AddValue(name, this.paramValues[i], typeof(object));
                }
            }
        }

        internal void SetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArrayList list = new ArrayList(20);
            this.methodName = info.GetString("__methodName");
            this.keyToNamespaceTable = (Hashtable) info.GetValue("__keyToNamespaceTable", typeof(Hashtable));
            ArrayList list2 = (ArrayList) info.GetValue("__paramNameList", typeof(ArrayList));
            this.xmlNameSpace = info.GetString("__xmlNameSpace");
            for (int i = 0; i < list2.Count; i++)
            {
                list.Add(info.GetValue((string) list2[i], Converter.typeofObject));
            }
            this.paramNames = new string[list2.Count];
            this.paramValues = new object[list.Count];
            for (int j = 0; j < list2.Count; j++)
            {
                this.paramNames[j] = (string) list2[j];
                this.paramValues[j] = list[j];
            }
        }

        public string[] FieldNames
        {
            get
            {
                return this.paramNames;
            }
            set
            {
                this.paramNames = value;
            }
        }

        public Type[] FieldTypes
        {
            get
            {
                return this.paramTypes;
            }
            set
            {
                this.paramTypes = value;
            }
        }
    }
}


namespace System.Runtime.Serialization
{
    using System;
    using System.Globalization;
    using System.Runtime;

    internal class DataNode<T> : IDataNode
    {
        private string clrAssemblyName;
        private string clrTypeName;
        private string dataContractName;
        private string dataContractNamespace;
        protected Type dataType;
        private string id;
        private bool isFinalValue;
        private T value;

        internal DataNode()
        {
            this.id = Globals.NewObjectId;
            this.dataType = typeof(T);
            this.isFinalValue = true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DataNode(T value) : this()
        {
            this.value = value;
        }

        internal void AddQualifiedNameAttribute(ElementData element, string elementPrefix, string elementName, string elementNs, string valueName, string valueNs)
        {
            string prefix = ExtensionDataReader.GetPrefix(valueNs);
            element.AddAttribute(elementPrefix, elementNs, elementName, string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { prefix, valueName }));
            bool flag = false;
            if (element.attributes != null)
            {
                for (int i = 0; i < element.attributes.Length; i++)
                {
                    AttributeData data = element.attributes[i];
                    if (((data != null) && (data.prefix == "xmlns")) && (data.localName == prefix))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                element.AddAttribute("xmlns", "http://www.w3.org/2000/xmlns/", prefix, valueNs);
            }
        }

        public virtual void Clear()
        {
            this.clrTypeName = (string) (this.clrAssemblyName = null);
        }

        public virtual void GetData(ElementData element)
        {
            element.dataNode = this;
            element.attributeCount = 0;
            element.childElementIndex = 0;
            if (this.DataContractName != null)
            {
                this.AddQualifiedNameAttribute(element, "i", "type", "http://www.w3.org/2001/XMLSchema-instance", this.DataContractName, this.DataContractNamespace);
            }
            if (this.ClrTypeName != null)
            {
                element.AddAttribute("z", "http://schemas.microsoft.com/2003/10/Serialization/", "Type", this.ClrTypeName);
            }
            if (this.ClrAssemblyName != null)
            {
                element.AddAttribute("z", "http://schemas.microsoft.com/2003/10/Serialization/", "Assembly", this.ClrAssemblyName);
            }
        }

        public T GetValue()
        {
            return this.value;
        }

        public string ClrAssemblyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.clrAssemblyName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.clrAssemblyName = value;
            }
        }

        public string ClrTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.clrTypeName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.clrTypeName = value;
            }
        }

        public string DataContractName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dataContractName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.dataContractName = value;
            }
        }

        public string DataContractNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dataContractNamespace;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.dataContractNamespace = value;
            }
        }

        public Type DataType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dataType;
            }
        }

        public string Id
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.id;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.id = value;
            }
        }

        public bool PreservesReferences
        {
            get
            {
                return (this.Id != Globals.NewObjectId);
            }
        }

        bool IDataNode.IsFinalValue
        {
            get
            {
                return this.isFinalValue;
            }
            set
            {
                this.isFinalValue = value;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = (T) value;
            }
        }
    }
}


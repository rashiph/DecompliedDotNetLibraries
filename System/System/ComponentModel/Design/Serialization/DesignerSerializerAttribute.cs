namespace System.ComponentModel.Design.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=true, Inherited=true)]
    public sealed class DesignerSerializerAttribute : Attribute
    {
        private string serializerBaseTypeName;
        private string serializerTypeName;
        private string typeId;

        public DesignerSerializerAttribute(string serializerTypeName, string baseSerializerTypeName)
        {
            this.serializerTypeName = serializerTypeName;
            this.serializerBaseTypeName = baseSerializerTypeName;
        }

        public DesignerSerializerAttribute(string serializerTypeName, Type baseSerializerType)
        {
            this.serializerTypeName = serializerTypeName;
            this.serializerBaseTypeName = baseSerializerType.AssemblyQualifiedName;
        }

        public DesignerSerializerAttribute(Type serializerType, Type baseSerializerType)
        {
            this.serializerTypeName = serializerType.AssemblyQualifiedName;
            this.serializerBaseTypeName = baseSerializerType.AssemblyQualifiedName;
        }

        public string SerializerBaseTypeName
        {
            get
            {
                return this.serializerBaseTypeName;
            }
        }

        public string SerializerTypeName
        {
            get
            {
                return this.serializerTypeName;
            }
        }

        public override object TypeId
        {
            get
            {
                if (this.typeId == null)
                {
                    string serializerBaseTypeName = this.serializerBaseTypeName;
                    int index = serializerBaseTypeName.IndexOf(',');
                    if (index != -1)
                    {
                        serializerBaseTypeName = serializerBaseTypeName.Substring(0, index);
                    }
                    this.typeId = base.GetType().FullName + serializerBaseTypeName;
                }
                return this.typeId;
            }
        }
    }
}


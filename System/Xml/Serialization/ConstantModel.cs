namespace System.Xml.Serialization
{
    using System;
    using System.Reflection;

    internal class ConstantModel
    {
        private System.Reflection.FieldInfo fieldInfo;
        private long value;

        internal ConstantModel(System.Reflection.FieldInfo fieldInfo, long value)
        {
            this.fieldInfo = fieldInfo;
            this.value = value;
        }

        internal System.Reflection.FieldInfo FieldInfo
        {
            get
            {
                return this.fieldInfo;
            }
        }

        internal string Name
        {
            get
            {
                return this.fieldInfo.Name;
            }
        }

        internal long Value
        {
            get
            {
                return this.value;
            }
        }
    }
}


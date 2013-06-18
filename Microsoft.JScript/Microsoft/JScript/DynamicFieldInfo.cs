namespace Microsoft.JScript
{
    using System;

    public sealed class DynamicFieldInfo
    {
        public string fieldTypeName;
        public string name;
        public object value;

        public DynamicFieldInfo(string name, object value)
        {
            this.name = name;
            this.value = value;
            this.fieldTypeName = "";
        }

        public DynamicFieldInfo(string name, object value, string fieldTypeName)
        {
            this.name = name;
            this.value = value;
            this.fieldTypeName = fieldTypeName;
        }
    }
}


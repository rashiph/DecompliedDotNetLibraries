namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataBindingHandlerAttribute : Attribute
    {
        private string _typeName;
        public static readonly DataBindingHandlerAttribute Default = new DataBindingHandlerAttribute();

        public DataBindingHandlerAttribute()
        {
            this._typeName = string.Empty;
        }

        public DataBindingHandlerAttribute(string typeName)
        {
            this._typeName = typeName;
        }

        public DataBindingHandlerAttribute(Type type)
        {
            this._typeName = type.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DataBindingHandlerAttribute attribute = obj as DataBindingHandlerAttribute;
            return ((attribute != null) && (string.Compare(this.HandlerTypeName, attribute.HandlerTypeName, StringComparison.Ordinal) == 0));
        }

        public override int GetHashCode()
        {
            return this.HandlerTypeName.GetHashCode();
        }

        public string HandlerTypeName
        {
            get
            {
                if (this._typeName == null)
                {
                    return string.Empty;
                }
                return this._typeName;
            }
        }
    }
}


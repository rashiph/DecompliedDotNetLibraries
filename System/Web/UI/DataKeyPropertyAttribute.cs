namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataKeyPropertyAttribute : Attribute
    {
        private readonly string _name;

        public DataKeyPropertyAttribute(string name)
        {
            this._name = name;
        }

        public override bool Equals(object obj)
        {
            DataKeyPropertyAttribute attribute = obj as DataKeyPropertyAttribute;
            return ((attribute != null) && string.Equals(this._name, attribute.Name, StringComparison.Ordinal));
        }

        public override int GetHashCode()
        {
            if (this.Name == null)
            {
                return 0;
            }
            return this.Name.GetHashCode();
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}


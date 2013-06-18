namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Reflection;

    internal sealed class PersonalizablePropertyEntry
    {
        private bool _isSensitive;
        private System.Reflection.PropertyInfo _propertyInfo;
        private PersonalizationScope _scope;

        public PersonalizablePropertyEntry(System.Reflection.PropertyInfo pi, PersonalizableAttribute attr)
        {
            this._propertyInfo = pi;
            this._scope = attr.Scope;
            this._isSensitive = attr.IsSensitive;
        }

        public bool IsSensitive
        {
            get
            {
                return this._isSensitive;
            }
        }

        public System.Reflection.PropertyInfo PropertyInfo
        {
            get
            {
                return this._propertyInfo;
            }
        }

        public PersonalizationScope Scope
        {
            get
            {
                return this._scope;
            }
        }
    }
}


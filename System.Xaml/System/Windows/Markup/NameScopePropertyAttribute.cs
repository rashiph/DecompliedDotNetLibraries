namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class NameScopePropertyAttribute : Attribute
    {
        private string _name;
        private System.Type _type;

        public NameScopePropertyAttribute(string name)
        {
            this._name = name;
        }

        public NameScopePropertyAttribute(string name, System.Type type)
        {
            this._name = name;
            this._type = type;
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

